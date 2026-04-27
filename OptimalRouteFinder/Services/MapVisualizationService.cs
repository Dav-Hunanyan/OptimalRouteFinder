using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OptimalRouteFinder.Models;
using System.Windows;
using System.Windows.Media.Animation;

namespace OptimalRouteFinder.Services
{
    public class MapVisualizationService
    {
        private MapGraph _graph;
        private Canvas? _canvas;
        private Dictionary<City, Ellipse> _cityShapes = new();

        public MapVisualizationService(MapGraph graph)
        {
            _graph = graph;
        }

        public void SetCanvas(Canvas c)
        {
            _canvas = c;
        }

        /// <summary>
        /// Render all cities and roads
        /// </summary>
        public void Render(City? startCity = null, City? endCity = null)
        {
            if (_canvas == null) return;
            _canvas.Children.Clear();
            _cityShapes.Clear();

            ArrangeCities(); // compute X/Y positions in a circular layout

            // draw roads first
            foreach (var a in _graph.Cities)
            {
                foreach (var (b, d) in _graph.Roads[a])
                {
                    if (a.Name.CompareTo(b.Name) >= 0) continue; // draw each road once
                    var line = new Line
                    {
                        X1 = a.X,
                        Y1 = a.Y,
                        X2 = b.X,
                        Y2 = b.Y,
                        Stroke = Brushes.DimGray,
                        StrokeThickness = 2
                    };
                    _canvas.Children.Add(line);

                    // distance label
                    var txt = new TextBlock
                    {
                        Text = d.ToString("0"),
                        FontSize = 10,
                        Foreground = Brushes.Black
                    };
                    Canvas.SetLeft(txt, (a.X + b.X) / 2);
                    Canvas.SetTop(txt, (a.Y + b.Y) / 2);
                    _canvas.Children.Add(txt);
                }
            }

            // draw cities
            foreach (var c in _graph.Cities)
            {
                Brush fill = Brushes.White; // default
                if (c == startCity) fill = Brushes.LightGreen;   // start city
                else if (c == endCity) fill = Brushes.OrangeRed; // end city

                var el = new Ellipse
                {
                    Width = 24,
                    Height = 24,
                    Fill = fill,
                    Stroke = Brushes.DimGray,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(el, c.X - 12);
                Canvas.SetTop(el, c.Y - 12);
                _canvas.Children.Add(el);
                _cityShapes[c] = el;

                var lbl = new TextBlock
                {
                    Text = c.Name,
                    FontSize = 12,
                    FontWeight = System.Windows.FontWeights.Bold
                };
                Canvas.SetLeft(lbl, c.X + 14);
                Canvas.SetTop(lbl, c.Y - 8);
                _canvas.Children.Add(lbl);
            }
        }

        /// <summary>
        /// Draws a path overlay on top of the map
        /// </summary>
        public async Task HighlightPathAsync(List<City> path)
        {
            if (_canvas == null || path == null || path.Count < 2) return;

            // Clear and redraw the map normally
            _canvas.Children.Clear();
            Render();

            var points = path.Select(c => new Point(c.X, c.Y)).ToList();

            // List to hold the "finished" green segments
            var finishedLines = new List<Polyline>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                var start = points[i];
                var end = points[i + 1];

                // line for the current segment
                var currentLine = new Polyline
                {
                    Stroke = Brushes.LightGreen,
                    StrokeThickness = 4,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                currentLine.Points.Add(start);
                _canvas.Children.Add(currentLine);

                // Animate this segment
                await AnimateSegment(currentLine, start, end, 0.8);

                // After finished, make it green
                currentLine.Stroke = Brushes.GreenYellow;
                finishedLines.Add(currentLine);
            }

            // Recolor start, end, must-visit/intermediate cities
            foreach (var c in _graph.Cities)
            {
                if (!_cityShapes.ContainsKey(c)) continue;
                var el = _cityShapes[c];
                if (c == path.First()) el.Fill = Brushes.LightGreen;
                else if (c == path.Last()) el.Fill = Brushes.Red;
                else if (path.Contains(c)) el.Fill = Brushes.DarkGray;
                else el.Fill = Brushes.White;
            }
        }

        /// <summary>
        /// Arrange city coordinates in a circular layout to avoid overlap
        /// </summary>
        private void ArrangeCities()
        {
            if (_canvas == null || _graph.Cities.Count == 0) return;

            double w = _canvas.ActualWidth;
            double h = _canvas.ActualHeight;

            int n = _graph.Cities.Count;
            double radius = Math.Min(w, h) / 2 - 40; // keep some padding
            double centerX = w / 2;
            double centerY = h / 2;

            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                var city = _graph.Cities[i];

                // Normalized position on a circle (0..1)
                city.NormX = 0.5 + 0.5 * Math.Cos(angle);
                city.NormY = 0.5 + 0.5 * Math.Sin(angle);

                // Actual pixel position
                city.X = centerX + radius * Math.Cos(angle);
                city.Y = centerY + radius * Math.Sin(angle);
            }
        }

        private Task AnimateSegment(Polyline line, Point start, Point end, double durationSeconds)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Add the start point if it's the first segment
            if (line.Points.Count == 0)
                line.Points.Add(start);

            var anim = new PointAnimation
            {
                From = start,
                To = end,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            anim.Completed += (s, e) =>
            {
                line.Points.Add(end);
                tcs.SetResult(true);
            };

            // We animate a "dummy" object and update line manually
            var dummy = new AnimatablePoint();
            dummy.PointChanged += (p) =>
            {
                if (line.Points.Count > 1)
                    line.Points[line.Points.Count - 1] = p;
                else
                    line.Points.Add(p);
            };

            dummy.BeginAnimation(AnimatablePoint.PointProperty, anim);

            return tcs.Task;
        }
    }
    public class AnimatablePoint : Animatable
    {
        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register(nameof(Point), typeof(Point), typeof(AnimatablePoint),
                new PropertyMetadata(new Point(), (d, e) =>
                {
                    var ap = (AnimatablePoint)d;
                    ap.PointChanged?.Invoke((Point)e.NewValue);
                }));

        public Point Point
        {
            get => (Point)GetValue(PointProperty);
            set => SetValue(PointProperty, value);
        }
        public event Action<Point> PointChanged;
        protected override Freezable CreateInstanceCore() => new AnimatablePoint();
    }
}
