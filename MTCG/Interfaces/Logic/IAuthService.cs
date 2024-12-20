using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IAuthService
{
    bool Register(string username, string password);
    User? Login(string username, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}