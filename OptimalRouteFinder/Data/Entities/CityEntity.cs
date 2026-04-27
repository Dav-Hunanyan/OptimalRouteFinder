namespace OptimalRouteFinder.Data.Entities
{
    public class CityEntity
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public Template Template { get; set; }
    }
}
