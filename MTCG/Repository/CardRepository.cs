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

        public bool AddCard(Card card)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 INSERT INTO cards (id, name, damage, type)
                                                                 VALUES (@id, @name, @damage, @type);
                                                                 """);

            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.String, card.Id);
            DataLayer.AddParameterWithValue(dbCommand, "@name", DbType.String, card.Name);
            DataLayer.AddParameterWithValue(dbCommand, "@damage", DbType.Int32, card.Damage);
            DataLayer.AddParameterWithValue(dbCommand, "@type", DbType.String, nameof(card.ElementType)); // Convert enum to String

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
