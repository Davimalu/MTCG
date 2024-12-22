using MTCG.Models.Cards;
using MTCG.Models.Enums;
using System.Data;

namespace MTCG.Repository
{
    public class RepositoryHelper
    {
        public Card CreateCardFromDatabaseEntry(IDataReader reader)
        {
            string id = reader.GetString(0);
            string name = reader.GetString(1);
            float damage = reader.GetFloat(2);
            ElementType elementType = Enum.Parse<ElementType>(reader.GetString(4));

            // Generate appropriate card type
            if (reader.GetString(3) == "Spell") // Spell Card
            {
                return new SpellCard(id, name, damage, elementType);
            }
            else // Monster Card
            {
                return new MonsterCard(id, name, damage, elementType);
            }
        }
    }
}
