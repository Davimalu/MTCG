using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Models;

namespace MTCG.Logic
{
    public class ScoreboardService : IScoreboardService
    {
        #region Singleton
        private static ScoreboardService? _instance;

        public static ScoreboardService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScoreboardService();
                }

                return _instance;
            }
        }
        #endregion
        #region DependencyInjection
        public ScoreboardService(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        public ScoreboardService() { }

        private readonly IUserService _userService = UserService.Instance;


        /// <summary>
        /// populates the scoreboard with the up-to-date user information
        /// </summary>
        /// <param name="scoreboard">the scoreboard object to be filled</param>
        public void FillScoreboard(Scoreboard scoreboard)
        {
            // Thread Safety: Ensure that no user information is changed / new users are added while the scoreboard is being generated
            lock (ThreadSync.UserLock)
            {
                List<string> allUsers = _userService.GetListOfUsers();

                // Get stats of each user
                foreach (string user in allUsers)
                {
                    User tmpUser = _userService.GetUserByName(user)!;

                    ScoreboardEntry newEntry = new ScoreboardEntry()
                    {
                        Username = tmpUser.Username,
                        ChosenName = tmpUser.DisplayName,
                        EloPoints = tmpUser.Stats.EloPoints,
                        Losses = tmpUser.Stats.Losses,
                        Ties = tmpUser.Stats.Ties,
                        Wins = tmpUser.Stats.Wins
                    };

                    scoreboard.Entries.Add(newEntry);
                }
            }

            // Sort list using Comparison<T> delegate: https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
            scoreboard.Entries.Sort((x, y) => y.EloPoints.CompareTo(x.EloPoints));
        }
    }
}
