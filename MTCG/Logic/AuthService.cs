using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    // Has to be moved inside the namespace for some reason
    using BCrypt.Net;

    public class AuthService
    {
        private static UserRepository userRepository = new UserRepository();

        public AuthService()
        {

        }

        public bool Register(string username, string password) {
            // Check if user already exists
            if (userRepository.GetUserByName(username) != null)
            {
                // User already exists
                return false;
            }
            
            // Create user

            // Hash password
            string hashedPassword = HashPassword(password);

            // Add user to database
            userRepository.AddUser(new User(username, hashedPassword));

            return true;
        }

        public User? Login(string username, string password)
        {
            // Check if user exists
            User? tempUser = userRepository.GetUserByName(username);

            if (tempUser != null)
            {
                if (VerifyPassword(password, tempUser.Password))
                {
                    // Generate token for user
                    string token = $"{tempUser.Username}-mtcgToken";
                    tempUser.AuthToken = token;

                    // Update authToken in Database
                    userRepository.UpdateUser(tempUser);

                    return tempUser;
                }
                else
                {
                    // Wrong credentials
                    return null;
                }
            }

            // User doesn't exist
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
