using MTCG.Models.Cards;

namespace MTCG.Models
{
    public class TradeOffer
    {
        public int? Id { get; set; }
        public required User User { get; set; }
        public required Card Card { get; set; }
        public bool RequestedMonster { get; set; }
        public float RequestedDamage { get; set; }
    }
}
