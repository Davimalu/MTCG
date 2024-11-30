using MTCG.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;
using System.Data;
using MTCG.Logic;

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
    }
}
