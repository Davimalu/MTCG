using MTCG.Models;

namespace MTCG.Interfaces;

public interface ICardService
{
    string GetCardType(Card card);
    string GetElementType(Card card);
    Card? GetCardById(string cardId);
    bool SaveCardToDatabase(Card card);
    bool UserOwnsCard(User user, Card card);
    bool UserHasCardInDeck(User user, Card card);
}