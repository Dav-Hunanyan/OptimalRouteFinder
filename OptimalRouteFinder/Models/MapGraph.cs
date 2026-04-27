namespace OptimalRouteFinder.Models
{
    public class MapGraph
    {
        public List<City> Cities { get; set; } = new();
        public Dictionary<City, List<(City, double)>> Roads { get; set; } = new();

        public void AddCity(City c)
        {
            if (!Cities.Contains(c))
            {
                Cities.Add(c);
                Roads[c] = new List<(City, double)>();
                // assign simple layout coordinates
                c.X = (Cities.Count * 70) % 600;
                c.Y = ((Cities.Count * 70) / 600) * 70 + 50;
            }
        }

        public void AddRoad(City a, City b, double distance)
        {
            if (!Cities.Contains(a)) AddCity(a);
            if (!Cities.Contains(b)) AddCity(b);
            Roads[a].Add((b, distance));
            Roads[b].Add((a, distance));
        }

        public IEnumerable<City> GetNeighbors(City c) => Roads.ContainsKey(c) ? Roads[c].Select(t=>t.Item1) : Enumerable.Empty<City>();
        public double GetDistance(City a, City b)
        {
            if (!Roads.ContainsKey(a)) return double.PositiveInfinity;
            var found = Roads[a].FirstOrDefault(t => t.Item1 == b);
            return found == default ? double.PositiveInfinity : found.Item2;
        }
    }
}
