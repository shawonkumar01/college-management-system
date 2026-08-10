using CollegeManagement.Core.Entities;
using CollegeManagement.Core.DTOs;

namespace CollegeManagement.Core.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        User? AuthenticateUser(string email, string password);
    }
}
