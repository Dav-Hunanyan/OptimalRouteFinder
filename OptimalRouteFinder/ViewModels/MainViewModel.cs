using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using OptimalRouteFinder.Models;
using OptimalRouteFinder.Algorithms;
using OptimalRouteFinder.Services;
using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Data;

namespace OptimalRouteFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private TemplateService _templateService;
        private MapGraph _graph = new();
        private Canvas? _canvas;
        private MapVisualizationService _viz;

        #region Properties        
        private readonly User _currentUser;
        public ObservableCollection<string> TemplateNames { get; set; } = new();

        private string _selectedTemplateName;
        public string SelectedTemplateName
        {
            get => _selectedTemplateName;
            set
            {
                if (_selectedTemplateName != value)
                {
                    _selectedTemplateName = value;
                    OnPropertyChanged(nameof(SelectedTemplateName));
                    LoadSelectedTemplateAsync();
                }
            }
        }

        private ObservableCollection<City> _cities = new ObservableCollection<City>();
        public ObservableCollection<City> Cities
        {
            get => _cities;
            set
            {
                if (_cities != value)
                {
                    _cities = value;
                    OnPropertyChanged(nameof(Cities));
                }
            }
        }

        public City? SelectedCity { get; set; }

        private string _newCityName = string.Empty;
        public string NewCityName
        {
            get => _newCityName;
            set
            {
                if (_newCityName != value)
                {
                    _newCityName = value;
                    OnPropertyChanged(nameof(NewCityName));
                    ((RelayCommand)AddCityCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private City _roadFrom;
        public City RoadFrom
        {
            get => _roadFrom;
            set
            {
                if (_roadFrom != value)
                {
                    _roadFrom = value;
                    OnPropertyChanged(nameof(RoadFrom));
                    OnPropertyChanged(nameof(RoadToOptions));
                    ((RelayCommand)AddRoadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private City _roadTo;
        public City RoadTo
        {
            get => _roadTo;
            set
            {
                if (_roadTo != value)
                {
                    _roadTo = value;
                    OnPropertyChanged(nameof(RoadTo));
                    OnPropertyChanged(nameof(RoadFromOptions));
                    ((RelayCommand)AddRoadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _roadDistance = string.Empty;
        public string RoadDistance
        {
            get => _roadDistance;
            set
            {
                if (_roadDistance != value)
                {
                    _roadDistance = value;
                    OnPropertyChanged(nameof(RoadDistance));
                    ((RelayCommand)AddRoadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private City _startCity;
        public City StartCity
        {
            get => _startCity;
            set
            {
                if (_startCity != value)
                {
                    _startCity = value;
                    OnPropertyChanged(nameof(StartCity));
                    OnPropertyChanged(nameof(MustVisitOptions));
                    ((RelayCommand)ComputeRouteCommand).RaiseCanExecuteChanged();
                    _viz.Render(StartCity, EndCity);
                }
            }
        }

        private City _endCity;
        public City EndCity
        {
            get => _endCity;
            set
            {
                if (_endCity != value)
                {
                    _endCity = value;
                    OnPropertyChanged(nameof(EndCity));
                    OnPropertyChanged(nameof(MustVisitOptions));
                    ((RelayCommand)ComputeRouteCommand).RaiseCanExecuteChanged();
                    _viz.Render(StartCity, EndCity);
                }
            }
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
                AddLog(value);
            }
        }

        private string _newTemplateName;
        public string NewTemplateName
        {
            get => _newTemplateName;
            set
            {
                if (_newTemplateName != value)
                {
                    _newTemplateName = value;
                    OnPropertyChanged(nameof(NewTemplateName));
                    ((RelayCommand)SaveTemplateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (_selectedTemplate != value)
                {
                    _selectedTemplate = value;
                    OnPropertyChanged(nameof(SelectedTemplate));
                    ((RelayCommand)LoadTemplateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteTemplateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _resultText = string.Empty;
        public string ResultText
        {
            get => _resultText;
            set
            {
                _resultText = value;
                OnPropertyChanged(nameof(ResultText));
            }
        }

        public ObservableCollection<string> LogLines { get; } = new();
        public ObservableCollection<RoadViewModel> Roads { get; } = new();

        public IEnumerable<City> RoadFromOptions => Cities.Except(RoadTo == null ? Enumerable.Empty<City>() : new[] { RoadTo });
        public IEnumerable<City> RoadToOptions => Cities.Except(RoadFrom == null ? Enumerable.Empty<City>() : new[] { RoadFrom });
        public IEnumerable<City> MustVisitOptions => Cities.Where(c => c != StartCity && c != EndCity);

        #endregion

        #region Commands

        public ICommand AddCityCommand { get; set; }
        public ICommand AddRoadCommand { get; set; }
        public ICommand ComputeRouteCommand { get; set; }
        public ICommand DeleteCityCommand { get; set; }
        public ICommand DeleteRoadCommand { get; set; }
        public ICommand ClearAllCommand { get; set; }
        public ICommand SaveTemplateCommand { get; set; }
        public ICommand LoadTemplateCommand { get; set; }
        public ICommand DeleteTemplateCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        #endregion

        #region Constructor

        public MainViewModel(TemplateService templateService, User currentUser)
        {
            _templateService = templateService;
            _currentUser = currentUser;

            Cities.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(RoadFromOptions));
                OnPropertyChanged(nameof(RoadToOptions));
                ((RelayCommand)AddRoadCommand).RaiseCanExecuteChanged();
            };

            InitializeCommands();

            _viz = new MapVisualizationService(_graph);
            LoadTemplateNamesAsync().GetAwaiter().GetResult();
            LoadRoads();
        }

        private void InitializeCommands()
        {
            AddCityCommand = new RelayCommand(_ => AddCity(), _ => !string.IsNullOrWhiteSpace(NewCityName));
            AddRoadCommand = new RelayCommand(_ => AddRoad(), _ => RoadFrom != null && RoadTo != null && double.TryParse(RoadDistance, out double _));
            ComputeRouteCommand = new RelayCommand(async _ => await ComputeRouteAsync(), _ => Cities.Count > 0 && StartCity != null && EndCity != null);
            DeleteCityCommand = new RelayCommand(obj => { if (obj is City city) DeleteCity(city); });
            DeleteRoadCommand = new RelayCommand(obj => { if (obj is Tuple<City, City, double> roadTuple) DeleteRoad(roadTuple.Item1, roadTuple.Item2); });
            ClearAllCommand = new RelayCommand(_ => ClearAll());
            SaveTemplateCommand = new RelayCommand(async _ => await SaveTemplateAsync(), _ => !string.IsNullOrWhiteSpace(NewTemplateName) && Cities.Any());
            LoadTemplateCommand = new RelayCommand(async _ => await LoadTemplateAsync(), _ => !string.IsNullOrWhiteSpace(SelectedTemplate));
            DeleteTemplateCommand = new RelayCommand(async _ => await DeleteTemplateAsync(), _ => !string.IsNullOrWhiteSpace(SelectedTemplate));
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        #endregion

        #region Service Call Methods

        private async Task LoadTemplateNamesAsync()
        {
            TemplateNames.Clear();
            var names = await _templateService.GetAllTemplateNamesAsync(_currentUser.Id);
            foreach (var name in names)
                TemplateNames.Add(name);
        }

        private async void LoadSelectedTemplateAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedTemplateName)) return;

            var (cities, roads) = await _templateService.LoadTemplateAsync(SelectedTemplateName);

            Cities.Clear();
            Roads.Clear();
            _graph.Cities.Clear();
            _graph.Roads.Clear();

            foreach (var c in cities)
            {
                Cities.Add(c);
                _graph.AddCity(c);
            }

            foreach (var r in roads)
            {
                Roads.Add(r);
                _graph.AddRoad(r.From, r.To, r.Distance);
            }

            _viz.Render(StartCity, EndCity);

            StatusText = $"«{SelectedTemplateName}» շաբլոնը բեռնվեց և պատկերվեց:";
        }

        private async Task SaveTemplateAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTemplateName))
            {
                StatusText = "Պահպանումը ձախողվեց. շաբլոնի անունը նշված չէ:";               
                return;
            }

            if (!Cities.Any())
            {
                StatusText = "Պահպանումը ձախողվեց. պահպանման ենթակա քաղաքներ չկան:";              
                return;
            }

            try
            {
                await _templateService.SaveTemplateAsync(NewTemplateName, Cities.ToList(), Roads.ToList());
                StatusText = $"«{NewTemplateName}» շաբլոնը հաջողությամբ պահպանվեց:";               
                await LoadTemplateNamesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Պահպանումը ձախողվեց. {ex.Message}";            
            }
        }

        private async Task LoadTemplateAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedTemplate))
            {
                StatusText = "Բեռնումը ձախողվեց. շաբլոն ընտրված չէ:";              
                return;
            }

            try
            {
                var (cities, roads) = await _templateService.LoadTemplateAsync(SelectedTemplate);

                if (!cities.Any())
                {
                    StatusText = $"Բեռնումը ձախողվեց. «{SelectedTemplate}» շաբլոնում քաղաքներ չկան:";                  
                    return;
                }

                Cities.Clear();
                Roads.Clear();
                _graph.Cities.Clear();
                _graph.Roads.Clear();

                foreach (var c in cities)
                {
                    Cities.Add(c);
                    _graph.AddCity(c);
                }

                foreach (var r in roads)
                {
                    Roads.Add(r);
                    _graph.AddRoad(r.From, r.To, r.Distance);
                }

                _viz.Render(StartCity, EndCity);
                NewTemplateName = SelectedTemplate;
                OnPropertyChanged(nameof(NewTemplateName));
                OnPropertyChanged(nameof(SelectedTemplate));
                StatusText = $"«{SelectedTemplate}» շաբլոնը հաջողությամբ բեռնվեց:";               
            }
            catch (Exception ex)
            {
                StatusText = $"Բեռնումը ձախողվեց. {ex.Message}";             
            }
        }

        private async Task DeleteTemplateAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedTemplate)) 
            {
                StatusText = "Ջնջումը ձախողվեց. շաբլոն ընտրված չէ:";               
                return;
            }

            try
            {
                await _templateService.DeleteTemplateAsync(SelectedTemplate);
                StatusText = $"«{SelectedTemplate}» շաբլոնը հաջողությամբ ջնջվեց:";              
                ClearAll();
                SelectedTemplate = null;
                await LoadTemplateNamesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Ջնջումը ձախողվեց. {ex.Message}";             
            }
        }

        #endregion

        #region ViewModel Methods

        private void AddLog(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogLines.Add(line);
        }

        public void SetCanvas(Canvas c)
        {
            _canvas = c;
            _viz.SetCanvas(c);
            _viz.Render();
        }

        public void AddCity()
        {
            var name = NewCityName.Trim();
            if (string.IsNullOrEmpty(name)) return;

            if (Cities.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                StatusText = "Քաղաքն արդեն գոյություն ունի:";
                return;
            }

            var city = new City { Name = name };
            Cities.Add(city);
            _graph.AddCity(city);
            NewCityName = string.Empty;
            OnPropertyChanged(nameof(NewCityName));
            OnPropertyChanged(nameof(MustVisitOptions));
            StatusText = $"«{city.Name}» քաղաքը ավելացվեց:";
            _viz.Render(StartCity, EndCity);
        }

        public void AddRoad()
        {
            if (RoadFrom == null || RoadTo == null) return;

            if (!double.TryParse(RoadDistance, out var distance))
            {
                StatusText = "Հեռավորության սխալ ձևաչափ:";
                return;
            }

            if (RoadFrom == RoadTo)
            {
                StatusText = "Մեկնարկային և վերջնական քաղաքները չեն կարող նույնը լինել:";
                return;
            }

            bool exists = (_graph.Roads.ContainsKey(RoadFrom) && _graph.Roads[RoadFrom].Any(x => x.Item1 == RoadTo))
                        || (_graph.Roads.ContainsKey(RoadTo) && _graph.Roads[RoadTo].Any(x => x.Item1 == RoadFrom));

            if (exists)
            {
                StatusText = $"«{RoadFrom.Name}» և «{RoadTo.Name}» քաղաքների միջև ճանապարհն արդեն գոյություն ունի:";
                return;
            }

            _graph.AddRoad(RoadFrom, RoadTo, distance);

            var roadVm = new RoadViewModel
            {
                From = RoadFrom,
                To = RoadTo,
                Distance = distance,
                DeleteCommand = new RelayCommand(_ => DeleteRoad(RoadFrom, RoadTo))
            };
            Roads.Add(roadVm);

            StatusText = $"Ճանապարհն ավելացվեց. {roadVm.Display}";
            _viz.Render(StartCity, EndCity);
        }

        private List<City> GetMustVisit() => Cities.Where(c => c.IsChecked && c != StartCity && c != EndCity).ToList();

        private void DeleteCity(City city)
        {
            if (city == null) return;

            var connected = Roads.Where(r => r.From == city || r.To == city).ToList();
            foreach (var road in connected) DeleteRoad(road.From, road.To);

            Cities.Remove(city);
            _graph.Cities.Remove(city);
            _graph.Roads.Remove(city);

            foreach (var other in _graph.Roads.Keys.ToList())
                _graph.Roads[other].RemoveAll(r => r.Item1 == city);

            LogLines.Add($"Քաղաքը ջնջվեց. {city.Name}");
            _viz.Render();
        }

        private void DeleteRoad(City from, City to)
        {
            if (_graph.Roads.ContainsKey(from)) _graph.Roads[from].RemoveAll(x => x.Item1 == to);
            if (_graph.Roads.ContainsKey(to)) _graph.Roads[to].RemoveAll(x => x.Item1 == from);

            var roadVm = Roads.FirstOrDefault(r => (r.From == from && r.To == to) || (r.From == to && r.To == from));
            if (roadVm != null) Roads.Remove(roadVm);

            LogLines.Add($"Ճանապարհը ջնջվեց. {from.Name} - {to.Name}");
            _viz.Render();
        }

        private async Task ComputeRouteAsync()
        {
            ResultText = "";
            OnPropertyChanged(nameof(ResultText));

            if (StartCity == null || EndCity == null)
            {
                StatusText = "Մեկնարկային կամ վերջնական կետը սահմանված չէ:";
                return;
            }

            var must = GetMustVisit();
            var nodes = new List<City>(must);
            if (!nodes.Contains(StartCity)) nodes.Insert(0, StartCity);
            if (!nodes.Contains(EndCity)) nodes.Add(EndCity);

            StatusText = "Հաշվարկվում են կարճագույն հեռավորությունները...";
            int n = nodes.Count;
            var dist = new double[n, n];
            var allPreds = new List<Dictionary<City, City>>();

            for (int i = 0; i < n; i++)
            {
                var (d, preds) = Dijkstra.Run(_graph, nodes[i]);
                allPreds.Add(preds);
                for (int j = 0; j < n; j++)
                    dist[i, j] = d.ContainsKey(nodes[j]) ? d[nodes[j]] : double.PositiveInfinity;
            }

            StatusText = "Որոնվում է օպտիմալ երթուղին (TSP)...";
            var (tspPath, tspCost) = HeldKarpTSP.Solve(dist, nodes, 0, n - 1);

            if (tspPath == null || tspPath.Count == 0)
            {
                StatusText = "Հնարավոր երթուղի չի գտնվել:";
                return;
            }

            List<City> Reconstruct(Dictionary<City, City> pred, City src, City dst)
            {
                var rev = new List<City>();
                var current = dst;
                while (!current.Equals(src))
                {
                    rev.Add(current);
                    if (!pred.TryGetValue(current, out current)) return new List<City>();
                }
                rev.Add(src);
                rev.Reverse();
                return rev;
            }

            var fullPath = new List<City>();
            for (int i = 0; i < tspPath.Count - 1; i++)
            {
                var a = tspPath[i];
                var b = tspPath[i + 1];
                int idx = nodes.IndexOf(a);
                var segment = Reconstruct(allPreds[idx], a, b);

                if (segment.Count == 0)
                {
                    StatusText = $"«{a.Name}» և «{b.Name}» քաղաքների միջև կապ չկա:";
                    return;
                }

                if (fullPath.Count == 0) fullPath.AddRange(segment);
                else { segment.RemoveAt(0); fullPath.AddRange(segment); }
            }

            ResultText = $"Երթուղի: {string.Join(" -> ", fullPath.Select(c => c.Name))}\nԸնդհանուր հեռավորությունը: {tspCost} կմ";
            OnPropertyChanged(nameof(ResultText));

            StatusText = "Պատրաստ է:";
            await _viz.HighlightPathAsync(fullPath);
        }

        private void ClearAll()
        {
            foreach (var c in Cities.ToList()) c.IsChecked = false;
            Cities.Clear();
            Roads.Clear();
            _graph.Cities.Clear();
            _graph.Roads.Clear();
            StartCity = null;
            EndCity = null;
            ResultText = string.Empty;
            NewTemplateName = string.Empty;
            SelectedTemplate = string.Empty;
            StatusText = "Տվյալները մաքրվեցին:";
            _viz.Render(StartCity, EndCity);
        }

        private void LoadRoads()
        {
            Roads.Clear();
            foreach (var from in _graph.Cities)
            {
                if (!_graph.Roads.ContainsKey(from)) continue;
                foreach (var (to, dist) in _graph.Roads[from])
                {
                    if (from.Name.CompareTo(to.Name) > 0) continue;
                    Roads.Add(new RoadViewModel
                    {
                        From = from,
                        To = to,
                        Distance = dist,
                        DeleteCommand = new RelayCommand(_ => DeleteRoad(from, to))
                    });
                }
            }
            _viz.Render(StartCity, EndCity);
        }
        private void Logout()
        {
            UserSession.CurrentUser = null;

            var loginWindow = new LoginView();
            loginWindow.Show();

            System.Windows.Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault()
                ?.Close();
        }
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    }
}
