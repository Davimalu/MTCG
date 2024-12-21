using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class TransactionsEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly IPackageService _packageService = PackageService.Instance;
        private readonly IStackService _stackService = StackService.Instance;

        private readonly IEventService _eventService = new EventService();
        private readonly IHttpHeaderService _ihttpHeaderService = new HttpHeaderService();

        public TransactionsEndpoint()
        {

        }

        #region DependencyInjection
        public TransactionsEndpoint(IUserService userService, IPackageService packageService, IStackService stackService, IEventService eventService, IHttpHeaderService ihttpHeaderService)
        {
            _userService = userService;
            _packageService = packageService;
            _stackService = stackService;
            _eventService = eventService;
            _ihttpHeaderService = ihttpHeaderService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            switch (headers.Method)
            {
                // Acquire Package
                case "POST":
                    return HandlePackageAcquiring(headers, user);
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }

        private (int, string?) HandlePackageAcquiring(HTTPHeader headers, User user)
        {
            // Buy new package
            if (headers is { Method: "POST", Path: "/transactions/packages" })
            {
                // Check if user has enough coins
                if (user.CoinCount < 5)
                {
                    return (402, JsonSerializer.Serialize("Not enough money"));
                }

                Package? package = _packageService.GetRandomPackage();
                if (package == null)
                {
                    return (410, JsonSerializer.Serialize("No packages available"));
                }

                // Add cards to user's stack
                _stackService.AddPackageToStack(package, user.Stack);

                // Save changes into database
                user.CoinCount -= 5;
                _userService.SaveUserToDatabase(user);

                var response = new
                {
                    message = "Package acquired",
                    AcquiredCards = package.Cards
                };

                _eventService.LogEvent(EventType.Highlight, $"User {user.Username} acquired a package!", null);
                return (201, JsonSerializer.Serialize(response));
            }

            return (404, "Not found");
        }
    }
}
