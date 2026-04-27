using System.Windows.Documents;

namespace OptimalRouteFinder.Data.Entities
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; }
        public List<CityEntity> Cities { get; set; } = new List<CityEntity>();
        public List<RoadEntity> Roads { get; set; } = new List<RoadEntity>();
    }
}
