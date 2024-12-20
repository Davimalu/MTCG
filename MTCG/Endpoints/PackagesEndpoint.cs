using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
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
        private readonly IHeaderHelper _headerHelper = new HeaderHelper();

        private readonly IEventService _eventService = new EventService();

        public PackagesEndpoint()
        {

        }

        #region DependencyInjection
        public PackagesEndpoint(ICardService cardService, IUserService userService, IPackageService packageService,
            IHeaderHelper headerHelper, IEventService eventService)
        {
            _cardService = cardService;
            _userService = userService;
            _packageService = packageService;
            _headerHelper = headerHelper;
            _eventService = eventService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _headerHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null || user.Username != "admin")
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }


            switch (headers.Method)
            {
                case "POST":
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
                if (_cardService.SaveCardToDatabase(card) && _packageService.AddCardToPackage(card, tmpPackage))
                {
                    cardsAdded++;
                }
            }

            // Check if all cards were successfully added
            if (cardsAdded != numberOfCards)
            {
                return (500, JsonSerializer.Serialize("Error writing cards to database"));
            }

            // Add package to database
            if (!_packageService.SavePackageToDatabase(tmpPackage))
            {
                return (500, JsonSerializer.Serialize("Error writing package to database"));
            }

            var response = new
            {
                message = "Package created successfully",
                AddedCards = tmpPackage.Cards
            };

            return (201, JsonSerializer.Serialize(response));
        }
    }
}
