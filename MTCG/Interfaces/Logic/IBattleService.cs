using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IBattleService
{
    string StartBattle(User playerA, User playerB);

    /// <summary>
    /// performs a round of combat between two playing cards
    /// </summary>
    /// <param name="cardA"></param>
    /// <param name="cardB"></param>
    /// <returns>
    /// <para>returns the card that won the round</para>
    /// <para>returns null on draw</para>
    /// </returns>
    Card? FightOneRound(Card cardA, Card cardB);

    Card? CardFight(Card cardA, Card cardB);

    /// <summary>
    /// retrieve a random card from a user's deck
    /// </summary>
    /// <param name="deck"></param>
    /// <returns>
    /// <para>a card object on success</para>
    /// <para>null if deck is empty or on error</para>
    /// </returns>
    Card? GetRandomCardFromDeck(Deck deck);

    Card? BattleWithSpecialties(Card cardA, Card cardB);
}