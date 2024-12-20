using MTCG.Models;

namespace MTCG.Logic;

public interface IScoreboardService
{
    void FillScoreboard(Scoreboard scoreboard);
}