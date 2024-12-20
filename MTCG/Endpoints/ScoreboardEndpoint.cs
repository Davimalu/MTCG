using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class ScoreboardEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly IScoreboardService _scoreboardService = ScoreboardService.Instance;
        private readonly IHeaderHelper _headerHelper = new HeaderHelper();

        public ScoreboardEndpoint()
        {

        }

        #region DependencyInjection
        public ScoreboardEndpoint(IUserService userService, IScoreboardService scoreboardService, IHeaderHelper headerHelper)
        {
            _userService = userService;
            _scoreboardService = scoreboardService;
            _headerHelper = headerHelper;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _headerHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            switch (headers.Method)
            {
                case "GET":
                    return HandleGetScoreboard();
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }

        private (int, string?) HandleGetScoreboard()
        {
            Scoreboard scoreboard = new Scoreboard();
            _scoreboardService.FillScoreboard(scoreboard);

            return (200, JsonSerializer.Serialize(scoreboard.Entries));
        }
    }
}
