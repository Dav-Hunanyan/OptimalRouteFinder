namespace OptimalRouteFinder.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Template> Templates { get; set; }
    }
}
