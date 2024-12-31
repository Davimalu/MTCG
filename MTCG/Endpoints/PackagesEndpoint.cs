using MTCG.HTTP;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class PackagesEndpoint : IHttpEndpoint
    {
        private readonly ICardService _cardService = CardService.Instance;
        private readonly IUserService _userService = UserService.Instance;
        private readonly IPackageService _packageService = PackageService.Instance;
        private readonly IAIService _aiService = AIService.Instance;
        private readonly IHttpHeaderService _ihttpHeaderService = new HttpHeaderService();

        private readonly IEventService _eventService = new EventService();

        public PackagesEndpoint()
        {

        }

        #region DependencyInjection
        public PackagesEndpoint(ICardService cardService, IUserService userService, IPackageService packageService,
            IHttpHeaderService ihttpHeaderService, IEventService eventService)
        {
            _cardService = cardService;
            _userService = userService;
            _packageService = packageService;
            _ihttpHeaderService = ihttpHeaderService;
            _eventService = eventService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null || user.Username != "admin")
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }


            switch (headers.Method)
            {
                case "POST":
                    if (headers.Path.Equals("/packages/ai"))
                    {
                        return HandleAddingPackageWithChatGpt(body);
                    }
                    return HandleAddingPackage(body);
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }


        private (int, string?) HandleAddingPackage(string? body)
        {
            // Check for valid input
            if (body == null)
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            // Each request contains an array of cards
            List<MonsterCard>? cardsToAdd = null;
            try
            {
                cardsToAdd = JsonSerializer.Deserialize<List<MonsterCard>>(body);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't parse request body of cards/package to add", ex);
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            if (cardsToAdd == null || cardsToAdd.Count != 5)
            {
                return (400, JsonSerializer.Serialize("Invalid request format"));
            }


            int numberOfCards = cardsToAdd.Count();
            int cardsAdded = 0;

            // Temporary package data structure used to save package into database
            Package tmpPackage = new Package();

            // Iterate over all cards
            foreach (var card in cardsToAdd)
            {
                // Add cards to database and temporary package
                var addedCard = _cardService.SaveCardToDatabase(card);

                if (addedCard != null && _packageService.AddCardToPackage(addedCard, tmpPackage))
                {
                    cardsAdded++;
                }
            }

            // Check if all cards were successfully added
            if (cardsAdded != numberOfCards)
            {
                // If 0 cards were added, that package probably already exists
                // If more than 0 but less than `numberOfCards` were added, there was an error adding SOME of the cards
                if (cardsAdded > 0)
                {
                    // Delete already added cards
                    foreach (var card in cardsToAdd)
                    {
                        _cardService.DeleteCardFromDatabase(card);
                    }
                    _eventService.LogEvent(EventType.Warning, $"Couldn't add package to database: Error saving some of the cards", null);
                    return (500, JsonSerializer.Serialize("Error writing cards to database"));
                }
                _eventService.LogEvent(EventType.Warning, $"Couldn't add package to database: Package already exists", null);
                return (400, JsonSerializer.Serialize("Package already exists"));
            }

            // Add package to database
            if (!_packageService.SavePackageToDatabase(tmpPackage))
            {
                // Delete already added cards
                foreach (var card in cardsToAdd)
                {
                    _cardService.DeleteCardFromDatabase(card);
                }
                return (500, JsonSerializer.Serialize("Error writing package to database"));
            }

            // Convert newly added cards into nicer format
            List<FrontendCard> addedCardsFancy = new List<FrontendCard>();
            foreach (Card card in tmpPackage.Cards)
            {
                addedCardsFancy.Add(_cardService.BackendCardToFrontendCard(card));
            }

            var response = new
            {
                message = "Package created successfully",
                AddedCards = addedCardsFancy
            };

            return (201, JsonSerializer.Serialize(response));
        }


        private (int, string?) HandleAddingPackageWithChatGpt(string? body)
        {
            // Check for valid input
            if (body == null)
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            // Check if all information was provided
            RequestBody? request = null;
            try
            {
                request = JsonSerializer.Deserialize<RequestBody>(body);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't parse request body of theme and API Key", ex);
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            if (request == null)
            {
                return (400, JsonSerializer.Serialize("Invalid request format"));
            }

            var cardsObject = _aiService.GetListOfCards(request.Theme, request.ApiKey);
            if (cardsObject == null)
            {
                return (500, JsonSerializer.Serialize("Received no response from the OpenAI API"));
            }

            // cardsObject contains a single property, which contains an array of cards
            // We need to get this array and check for errors on the way
            CardsWrapper? cardsWrapper = null;
            try
            {
                cardsWrapper = JsonSerializer.Deserialize<CardsWrapper>(cardsObject);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Couldn't parse response from the OpenAI API: Invalid Response", ex);
            }

            if (cardsWrapper == null)
            {
                return (500, JsonSerializer.Serialize("Received an invalid from the OpenAI API"));
            }

            // HandleAddingPackage(string? body) requires a JSON String containing an array of cards as its argument
            string? cardsToAdd = null;
            try
            {
                cardsToAdd = JsonSerializer.Serialize(cardsWrapper.Cards);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Received an invalid response from the OpenAI API", ex);
            }

            if (cardsToAdd == null)
            {
                return (500, JsonSerializer.Serialize("Received an invalid from the OpenAI API"));
            }

            return HandleAddingPackage(cardsToAdd);
        }
    }


    /// <summary>
    /// Wrapper class for deserialization of the request body of requests to /packages/ai
    /// </summary>
    internal class RequestBody
    {
        public required string Theme { get; set; }
        public required string ApiKey { get; set; }
    }


    /// <summary>
    /// Wrapper class for deserialization of the OpenAI API response since the OpenAI API can only return objects, not arrays
    /// </summary>
    internal class CardsWrapper
    {
        public List<MonsterCard> Cards { get; set; } = new List<MonsterCard>();
    }
}
