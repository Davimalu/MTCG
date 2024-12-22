using MTCG.Models;
using MTCG.Models.Cards;

namespace MTCG.Interfaces.Logic;

public interface IDeckService
{
    /// <summary>
    /// adds a card to the user's deck
    /// </summary>
    /// <param name="card">the card to add</param>
    /// <param name="deck">the deck of the user</param>
    /// <returns>
    /// <para>true if card was added to deck</para>
    /// <para>false if card couldn't be added to deck (e.g. because there were already 4 cards in the deck)</para>
    /// </returns>
    bool AddCardToUserDeck(Card card, Deck deck);

    /// <summary>
    /// removes a card from the user's deck
    /// </summary>
    /// <param name="cardToRemove">the card to remove</param>
    /// <param name="deck">the deck of the user</param>
    void RemoveCardFromUserDeck(Card cardToRemove, Deck deck);

    /// <summary>
    /// serializes a given deck into a human-readable plaintext representation
    /// </summary>
    /// <param name="deck"></param>
    /// <returns></returns>
    string SerializeDeckToPlaintext(Deck deck);
}