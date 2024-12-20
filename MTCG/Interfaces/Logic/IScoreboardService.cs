using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IScoreboardService
{
    void FillScoreboard(Scoreboard scoreboard);
}