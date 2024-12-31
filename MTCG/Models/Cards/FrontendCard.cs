namespace MTCG.Models.Cards
{
    /// <summary>
    /// Simplified version of the Card class, whose attributes are all human-readable
    /// </summary>
    public class FrontendCard
    {
        public required string CardId { get; set; }
        public required string CardName { get; set; }
        public required float Damage { get; set; }
        public required string CardType { get; set; }
        public required string ElementType { get; set; }
    }
}
