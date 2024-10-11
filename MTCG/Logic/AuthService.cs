using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    // Has to be moved inside the namespace for some reason
    using BCrypt.Net;

    public class AuthService
    {
        public static List<User> registeredUsers = new List<User>();

        public AuthService()
        {

        }

        public bool Register(string username, string password) {
            // Check if user already exists
            foreach (User user in registeredUsers)
            {
                if (user.Username == username)
                {
                    // User already exists
                    return false;
                }
            }
            // Create user

            // Hash password
            string hashedPassword = HashPassword(password);

            // Add user to list (replace with database)
            registeredUsers.Add(new User(username, hashedPassword));

            return true;
        }

        public User? Login(string username, string password)
        {
            // Check if user exists
            foreach (User user in registeredUsers)
            {
                if (user.Username == username)
                {
                    // Check if password is correct
                    if (VerifyPassword(password, user.Password))
                    {
                        // Generate unique token for user
                        string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                        user.authToken = token;
                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Verify(password, hash);
        }
    }
}
