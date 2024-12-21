using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IAuthService
{
    bool RegisterUser(string username, string password);
    User? LoginUser(string username, string password);
}