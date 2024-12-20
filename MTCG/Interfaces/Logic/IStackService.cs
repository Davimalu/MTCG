using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IStackService
{
    void AddCardToStack(Card card, Stack stack);
    void AddPackageToStack(Package package, Stack stack);
    bool RemoveCardFromStack(Card card, Stack stack);
}