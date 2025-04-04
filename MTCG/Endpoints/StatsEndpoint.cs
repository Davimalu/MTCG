﻿using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Logic;
using MTCG.Models;
using System.Net.Sockets;
using System.Text.Json;
using MTCG.Interfaces.Logic;

namespace MTCG.Endpoints
{
    public class StatsEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly IHttpHeaderService _httpHeaderService = new HttpHeaderService();

        public StatsEndpoint()
        {

        }

        #region DependencyInjection
        public StatsEndpoint(IUserService userService, IHttpHeaderService httpHeaderService)
        {
            _userService = userService;
            _httpHeaderService = httpHeaderService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _httpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            switch (headers.Method)
            {
                // Get user stats
                case "GET":
                    return (200, JsonSerializer.Serialize(user.Stats));
                default:
                    return (405, "Method not allowed");
            }
        }
    }
}
