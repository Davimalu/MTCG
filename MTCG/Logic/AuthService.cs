using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    // Has to be moved inside the namespace for some reason
    using BCrypt.Net;
    using MTCG.DAL;
    using MTCG.Interfaces;

    public class AuthService
    {
        #region Singleton
        private static AuthService? instance;

        public static AuthService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AuthService();
                }

                return instance;
            }
        }
        #endregion

        private readonly IUserRepository _userRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        // Dependency Injection über den Konstruktor
        public AuthService(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }

        public bool Register(string username, string password) {
            // Check if user already exists
            if (_userRepository.GetUserByName(username) != null)
            {
                // User already exists
                return false;
            }
            
            // Create user

            // Hash password
            string hashedPassword = HashPassword(password);

            // Add user to database
            _userRepository.AddUser(new User(username, hashedPassword));

            return true;
        }

        public User? Login(string username, string password)
        {
            // Check if user exists
            User? tempUser = _userRepository.GetUserByName(username);

            if (tempUser != null)
            {
                if (VerifyPassword(password, tempUser.Password))
                {
                    // Generate token for user
                    string token = $"{tempUser.Username}-mtcgToken";
                    tempUser.AuthToken = token;

                    // Update authToken in Database
                    _userRepository.UpdateUser(tempUser);

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

        public User? CheckAuthorization(HTTPHeader headers)
        {
            // Provided authorization string should be something like "Bearer admin-mtcgToken"

            // Check for correct number of words in string
            string authString = headers.Headers["Authorization"];
            if (headers.Headers["Authorization"].Split(' ').Length != 2)
            {
                return null;
            }

            // Check token against database
            return _userRepository.GetUserByToken(authString.Split(' ')[1]);

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
