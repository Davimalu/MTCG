using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using MTCG.Repository;
using System.Text.Json;

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
        private readonly ICardService _cardService = CardService.Instance;

        private readonly IEventService _eventService = new EventService();

        public int? SaveUserToDatabase(User user)
        {
            // If the user object already has a userId associated with it, there's already an entry in the database for it
            if (user.Id != null)
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


        public User? GetUserByName(string username)
        {
            // Get static user information
            User? user = _userRepository.GetUserByName(username);

            if (user == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve user {username} from database: User doesn't exist", null);
                return null;
            }

            user = AddStackToUser(user);
            user = AddDeckToUser(user);

            return user;
        }


        public User? GetUserByToken(string token)
        {
            // Get static user information
            User? user = _userRepository.GetUserByToken(token);

            if (user == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve user by token {token} from database: User doesn't exist", null);
                return null;
            }

            user = AddStackToUser(user);
            user = AddDeckToUser(user);

            return user;
        }


        public User? GetUserById(int userId)
        {
            // Get static user information
            User? user = _userRepository.GetUserById(userId);

            if (user == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve user with ID {userId} from database: User doesn't exist", null);
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
                Card? cardToAdd = _cardService.GetCardById(cardId);

                if (cardToAdd == null)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't add Card to Stack of user {user.Username}: Card doesn't exist", null);
                    continue;
                }

                userStack.Cards.Add(cardToAdd);
            }
            user.Stack = userStack;

            return user;
        }


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
                Card? cardToAdd = _cardService.GetCardById(cardId);

                if (cardToAdd == null)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't add Card to Deck of user {user.Username}: Card doesn't exist", null);
                    continue;
                }

                userDeck.Cards.Add(cardToAdd);
            }
            user.Deck = userDeck;

            return user;
        }


        public List<string> GetListOfUsers()
        {
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
