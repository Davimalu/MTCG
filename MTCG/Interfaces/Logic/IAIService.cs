namespace MTCG.Interfaces.Logic;

public interface IAIService
{
    string? GetListOfCards(string theme, string apiKey);
}