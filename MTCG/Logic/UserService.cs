using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class UserService
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
        private readonly CardRepository _cardRepository = CardRepository.Instance;

        public void UpdateUser(User user)
        {
            // Update static parameters
            _userRepository.UpdateUser(user);

            // Update stack of user
            _userRepository.SaveStackOfUser(user);
        }

        public User? GetUserByToken(string token)
        {
            // Get static user information
            User? user = _userRepository.GetUserByToken(token);

            if (user == null)
            {
                return null;
            }

            // Get the IDs of the cards the user has in his stack
            List<string>? cardIds = _userRepository.GetCardIdsOfUserStack(user);

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

            // TODO: Also retrieve the user's deck

            return user;
        }
    }
}
