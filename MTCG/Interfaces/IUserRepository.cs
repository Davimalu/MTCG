using MTCG.DAL;
using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);
        User? GetUserById(int id);
        User? GetUserByName(string username);
        void UpdateUser(User user);
        User? GetUserByToken(string token);
    }
}