using MTCG.Models;
using MTCG.Models.Cards;

namespace MTCG.Interfaces.Logic;

public interface IStackService
{
    void AddCardToStack(Card card, Stack stack);
    void AddPackageToStack(Package package, Stack stack);
    /// <summary>
    /// removes a card from a users' stack
    /// </summary>
    /// <param name="card">the card to be removed</param>
    /// <param name="stack">the users' stack</param>
    /// <returns>
    /// <para>true if the card was successfully removed</para>
    /// <para>false on error of if the user didn't own the card</para>
    /// </returns>
    bool RemoveCardFromStack(Card card, Stack stack);
}