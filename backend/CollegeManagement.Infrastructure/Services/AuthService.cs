using CollegeManagement.Core.Entities;
using CollegeManagement.Core.Services;
using CollegeManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CollegeManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public string GenerateJwtToken(User user)
        {
            throw new NotImplementedException("JWT token generation should be in the API layer");
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public User? AuthenticateUser(string email, string password)
        {
            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }
    }
}
