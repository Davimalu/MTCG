using MTCG.DAL;
using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IUserRepository
    {
        int SaveUserToDatabase(User user);
        User? GetUserByName(string username);
        int UpdateUser(User user);
        User? GetUserByToken(string token);
    }
}