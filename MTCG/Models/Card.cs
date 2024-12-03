using static MTCG.Models.Card;

namespace MTCG.Models
{
    // Enum for element type
    public enum ElementType
    {
        Fire,
        Water,
        Normal
    }

    public abstract class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Damage { get; set; }
        public float TemporaryDamage { get; set; }
        public ElementType ElementType { get; set; }

        public Card(string id, string name, float damage, ElementType elementType)
        {
            this.Id = id;
            this.Name = name;
            this.Damage = damage;
            this.ElementType = elementType;
        }

        public Card() // Parameterless constructor is required for deserialization with System.Text.JSON
        {
            // TODO: Replace this
            ElementType = ElementType.Fire;
        } 

        public abstract void PrintCard();
    }
}