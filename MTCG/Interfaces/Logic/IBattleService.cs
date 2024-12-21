using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IBattleService
{
    string? StartBattle(User playerA, User playerB);
}