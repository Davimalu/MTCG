using MTCG.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;
using System.Data;
using MTCG.Logic;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace MTCG.Repository
{
    public class CardRepository
    {
        #region Singleton
        private static CardRepository? instance;

        public static CardRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CardRepository();
                }

                return instance;
            }
        }
        #endregion

        private readonly DataLayer _dataLayer = DataLayer.Instance;
        private readonly CardService _cardService = CardService.Instance;

        public bool AddCard(Card card)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 INSERT INTO cards (cardId, name, damage, cardType, elementType)
                                                                 VALUES (@id, @name, @damage, @cardType, @elementType);
                                                                 """);

            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.String, card.Id);
            DataLayer.AddParameterWithValue(dbCommand, "@name", DbType.String, card.Name);
            DataLayer.AddParameterWithValue(dbCommand, "@damage", DbType.Int32, card.Damage);
            DataLayer.AddParameterWithValue(dbCommand, "@cardType", DbType.String, _cardService.GetCardType(card));
            DataLayer.AddParameterWithValue(dbCommand, "@elementType", DbType.String, _cardService.GetElementType(card));

            int rowsAffected = dbCommand.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                Console.WriteLine($"[INFO] Card {card.Name} added to database!");
                return true;
            }
            else
            {
                Console.WriteLine($"[Error] Card {card.Name} couldn't be added to database!");
                return false;
            }
        }

        public Card? GetCardById(string cardId)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT cardId, name, damage, cardType, elementType
                                                                  FROM cards
                                                                  WHERE cardId = @cardId
                                                                  """);
            DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, cardId);

            using IDataReader reader = dbCommand.ExecuteReader();

            if (reader.Read())
            {

                Console.WriteLine($"[INFO] Card {reader.GetString(1)} retrieved from database!");

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

            return null;
        }
    }
}
