using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OptimalRouteFinder.Services
{
    public class UserService
    {
        private readonly MapDbContext _dbContext;

        public UserService(MapDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool UsernameExists(string username)
        {
            return _dbContext.Users.Any(u => u.Username == username);
        }

        public User RegisterUser(string username, string email, string password)
        {
            if (UsernameExists(username))
                throw new ApplicationException("Username already exists");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
