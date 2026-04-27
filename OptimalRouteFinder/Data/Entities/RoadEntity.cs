namespace OptimalRouteFinder.Data.Entities
{
    public class RoadEntity
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int FromCityId { get; set; }
        public int ToCityId { get; set; }
        public double Distance { get; set; }
        public Template Template { get; set; }
        public CityEntity FromCity { get; set; }
        public CityEntity ToCity { get; set; }
    }
}
