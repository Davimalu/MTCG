using System.Text.Json.Serialization;
using MTCG.Models.Enums;

namespace MTCG.Models.Cards
{
    public abstract class Card
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Damage { get; set; }
        [JsonIgnore]
        public float TemporaryDamage { get; set; }
        public ElementType ElementType { get; set; }
    }
}