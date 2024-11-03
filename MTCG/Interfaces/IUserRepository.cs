using MTCG.DAL;
using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);
        DataLayer GetDataLayer();
        User? GetUserById(int id);
        User? GetUserByName(string username);
        void UpdateUser(User user);
    }
}