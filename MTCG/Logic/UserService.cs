using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Interfaces;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class UserService : IUserService
    {
        #region Singleton
        private static UserService? _instance;

        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserService();
                }

                return _instance;
            }
        }
        #endregion

        private readonly UserRepository _userRepository = UserRepository.Instance;
        private readonly StackRepository _stackRepository = StackRepository.Instance;
        private readonly DeckRepository _deckRepository = DeckRepository.Instance;
        private readonly CardRepository _cardRepository = CardRepository.Instance;

        /// <summary>
        /// saves a new user to the database or - if the user already exists - updates his information
        /// </summary>
        /// <param name="user">user object | Stack and Deck are optional</param>
        /// <returns>ID of the newly created or updated database entry</returns>
        public int SaveUserToDatabase(User user)
        {
            // If the user object already has a userId associated with it, there's already an entry in the database for it
            if (user.Id != 0)
            {
                user.Id = _userRepository.UpdateUser(user);
            }
            else
            {
                user.Id = _userRepository.SaveUserToDatabase(user);
            }

            // If a stack is associated with the user, add it to the database
            if (user.Stack.Cards.Count > 0)
            {
                _stackRepository.ClearUserStack(user);
                _stackRepository.SaveStackOfUser(user);
            }

            // If a deck is associated with the user, add it to the database
            if (user.Deck.Cards.Count > 0)
            {
                _deckRepository.ClearUserDeck(user);
                _deckRepository.SaveDeckOfUser(user);
            }
            
            return user.Id;
        }

        /// <summary>
        /// retrieve a user from the database using his unique username
        /// </summary>
        /// <param name="username">the username of the user</param>
        /// <returns>
        /// <para>user object containing all information + deck and stack on success</para>
        /// <para>null if there is no user with that username or an error occured</para>
        ///</returns>
        public User? GetUserByName(string username)
        {
            // Get static user information
            User? user = _userRepository.GetUserByName(username);

            if (user == null)
            {
                return null;
            }

            user = AddStackToUser(user);
            user = AddDeckToUser(user);

            return user;
        }

        /// <summary>
        /// retrieve a user from the database using his authentication token
        /// </summary>
        /// <param name="token">the authentication token in format "xxx-mtcgToken"</param>
        /// <returns>
        /// <para>user object containing all information + deck and stack on success</para>
        /// <para>null if there is no user with that token or an error occured</para>
        /// </returns>
        public User? GetUserByToken(string token)
        {
            // Get static user information
            User? user = _userRepository.GetUserByToken(token);

            if (user == null)
            {
                return null;
            }

            user = AddStackToUser(user);
            user = AddDeckToUser(user);

            return user;
        }

        /// <summary>
        /// retrieve a user from the database using his unique Id
        /// </summary>
        /// <param name="userId">the id of the user</param>
        /// <returns>
        /// <para>user object containing all information + deck and stack on success</para>
        /// <para>null if there is no user with that Id or an error occured</para>
        ///</returns>
        public User? GetUserById(int userId)
        {
            // Get static user information
            User? user = _userRepository.GetUserById(userId);

            if (user == null)
            {
                return null;
            }

            user = AddStackToUser(user);
            user = AddDeckToUser(user);

            return user;
        }

        public User AddStackToUser(User user)
        {
            // Get the IDs of the cards the user has in his stack
            List<string>? cardIds = _stackRepository.GetCardIdsOfUserStack(user);

            if (cardIds == null || cardIds.Count == 0)
            {
                // User has no cards in his stack
                return user;
            }

            // Fill user's stack with cards
            Stack userStack = new Stack();
            foreach (string cardId in cardIds)
            {
                userStack.Cards.Add(_cardRepository.GetCardById(cardId));
            }
            user.Stack = userStack;

            return user;
        }

        // TODO: This function is very similar to AddStackToUser -> Refactor
        public User AddDeckToUser(User user)
        {
            // Get the IDs of the cards the user has in his deck
            List<string>? cardIds = _deckRepository.GetCardIdsOfUserDeck(user);

            if (cardIds == null || cardIds.Count == 0)
            {
                // User has no cards in his deck
                return user;
            }

            // Fill user's deck with cards
            Deck userDeck = new Deck();
            foreach (string cardId in cardIds)
            {
                userDeck.Cards.Add(_cardRepository.GetCardById(cardId));
            }
            user.Deck = userDeck;

            return user;
        }

        /// <summary>
        /// returns a list of usernames of all users currently registered to the game
        /// </summary>
        /// <returns>
        /// <para>A list of strings containing the usernames (unique) of each user</para>
        /// </returns>
        public List<string> GetListOfUsers()
        {
            // TODO: Is this function necessary? It's purpose is so that no one but the userService interacts with userRepository
            return _userRepository.GetListOfUsers();
        }

        public string UserToJson(User user)
        {
            var jsonObject = new
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                Biography = user.Biography,
                Image = user.Image,
                Stats = new
                {
                    Wins = user.Stats.Wins,
                    Losses = user.Stats.Losses,
                    Ties = user.Stats.Ties,
                    EloPoints = user.Stats.EloPoints
                }
            };

            return JsonSerializer.Serialize(jsonObject);
        }

        public void UpdateUserStats(User winner, User loser, bool tie)
        {
            // Empty deck since we don't want to store the deck updated by the battle in the database
            winner.Deck = new Deck();
            loser.Deck = new Deck();

            if (tie == true)
            {
                winner.Stats.Ties += 1;
                loser.Stats.Ties += 1;
            }
            else
            {
                winner.Stats.EloPoints += 3;
                loser.Stats.EloPoints -= 5;

                winner.Stats.Wins += 1;
                loser.Stats.Losses += 1;
            }

            SaveUserToDatabase(winner);
            SaveUserToDatabase(loser);
        }
    }
}
