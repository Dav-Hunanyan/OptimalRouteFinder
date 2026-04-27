using OptimalRouteFinder.Algorithms;
using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Data;

namespace OptimalRouteFinder.Services
{
    public class AuthService
    {
        private readonly MapDbContext _context;

        public AuthService(MapDbContext context)
        {
            _context = context;
        }

        public User Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            var users = _context.Users.ToList();
            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}
