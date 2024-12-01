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

        /// <summary>
        /// saves a new card to the database
        /// </summary>
        /// <param name="card"></param>
        /// <returns>
        /// <para>true if card was successfully added to database</para>
        /// <para>false if card couldn't be added to database</para>
        /// </returns>>
        public bool AddCardToDatabase(Card card)
        {
            // Prepare SQL Statement
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 INSERT INTO cards (cardId, name, damage, cardType, elementType)
                                                                 VALUES (@id, @name, @damage, @cardType, @elementType);
                                                                 """);

            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.String, card.Id);
            DataLayer.AddParameterWithValue(dbCommand, "@name", DbType.String, card.Name);
            DataLayer.AddParameterWithValue(dbCommand, "@damage", DbType.Int32, card.Damage);
            DataLayer.AddParameterWithValue(dbCommand, "@cardType", DbType.String, _cardService.GetCardType(card));
            DataLayer.AddParameterWithValue(dbCommand, "@elementType", DbType.String, _cardService.GetElementType(card));

            // Execute query and catch errors
            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Card {card.Name} couldn't be added to database!");
                Console.WriteLine($"[ERROR] {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves a card - identified by its cardId - from the database
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns>
        /// <para>Monster- or Spellcard object</para>
        /// <para>null if there is no card in the database with the specified Id</para>
        /// </returns>
        public Card? GetCardById(string cardId)
        {
            // Prepare SQL Statement
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT cardId, name, damage, cardType, elementType
                                                                  FROM cards
                                                                  WHERE cardId = @cardId
                                                                  """);
            DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, cardId);

            // Execute query
            // TODO: Add error handling?
            using IDataReader reader = dbCommand.ExecuteReader();

            // Check if query returned a result
            if (reader.Read())
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

            // Return null if no results
            return null;
        }
    }
}
