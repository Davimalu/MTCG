using MTCG.HTTP;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class CardsEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly ICardService _cardService = CardService.Instance;
        private readonly IHttpHeaderService _ihttpHeaderService = new HttpHeaderService();

        public CardsEndpoint()
        {

        }

        #region DependencyInjection
        public CardsEndpoint(IUserService userService, IHttpHeaderService ihttpHeaderService)
        {
            _userService = userService;
            _ihttpHeaderService = ihttpHeaderService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            // Check if user is authorized
            string? token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }


            switch (headers.Method)
            {
                // Get list of cards
                case "GET":
                    return (200, _cardService.SerializeCardsToJson(user.Stack.Cards));
                default:
                    return (405, JsonSerializer.Serialize("Method not allowed"));
            }
        }
    }
}
