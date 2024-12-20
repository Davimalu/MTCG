using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class CardsEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly IHeaderHelper _headerHelper = new HeaderHelper();

        public CardsEndpoint()
        {

        }

        #region DependencyInjection
        public CardsEndpoint(IUserService userService, IHeaderHelper headerHelper)
        {
            _userService = userService;
            _headerHelper = headerHelper;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string? token = _headerHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }


            switch (headers.Method)
            {
                // Get list of cards
                case "GET":
                    return (200, JsonSerializer.Serialize(user.Stack));
                default:
                    return (405, JsonSerializer.Serialize("Method not allowed"));
            }
        }
    }
}
