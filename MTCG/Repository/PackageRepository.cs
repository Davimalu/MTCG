using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.DAL;
using MTCG.Models;

namespace MTCG.Repository
{
    public class PackageRepository
    {
        #region Singleton
        private static PackageRepository? _instance;

        public static PackageRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PackageRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DataLayer _dataLayer = DataLayer.Instance;

        public bool AddPackage(Package package)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("INSERT INTO packages DEFAULT VALUES RETURNING packageId;");

            package.Id = (int)(dbCommand.ExecuteScalar() ?? 0);

            // Check if query executed successfully
            if (package.Id == 0)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[ERROR] Package couldn't be added to database!");
                Console.ResetColor();
                return false;
            }

            Console.WriteLine($"[INFO] New package with ID {package.Id} added to database!");
            return AddCardsToPackage(package);
        }

        public bool AddCardsToPackage(Package package)
        {
            foreach (var card in package.Cards)
            {
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      INSERT INTO cardsPackages (packageId, cardId)
                                                                      VALUES (@packageId, @cardId);
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, package.Id);
                DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, card.Id);

                int rowsAffected = dbCommand.ExecuteNonQuery();

                if (rowsAffected <= 0)
                {
                    Console.WriteLine($"[Error] Card {card.Name} couldn't be added to database!");
                    return false;
                }
            }

            return true;
        }

        public int GetRandomPackageId()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT packageId FROM packages
                                                                  ORDER BY RANDOM()
                                                                  LIMIT 1
                                                                  """);

            using IDataReader reader = dbCommand.ExecuteReader();

            if (reader.Read())
            {
                return reader.GetInt32(0);
            }

            return 0;
        }

        public Package? GetPackageFromId(int packageId)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT cards.cardId, name, damage, cardType, elementType FROM cards
                                                                  JOIN cardsPackages USING (cardId)
                                                                  WHERE cardsPackages.packageId = @packageId
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, packageId);

            using IDataReader reader = dbCommand.ExecuteReader();

            List<Card> cards = new List<Card>(5);

            while (reader.Read())
            {
                string id = reader.GetString(0);
                string name = reader.GetString(1);
                float damage = reader.GetFloat(2);
                ElementType elementType = Enum.Parse<ElementType>(reader.GetString(4));

                // Generate appropriate card type
                if (reader.GetString(3) == "Spell") // Spell Card
                {
                    cards.Add(new SpellCard(id, name, damage, elementType));
                }
                else // Monster Card
                {
                    cards.Add(new MonsterCard(id, name, damage, elementType));
                }
            }

            // If the package doesn't exist or has no cards associated with it...
            if (cards.Count == 0)
            {
                return null;
            }

            return new Package()
            {
                Id = packageId,
                Cards = cards
            };
        }

        public bool DeletePackageById(int packageId)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  DELETE FROM packages
                                                                  WHERE packageId = @packageId;
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, packageId);

            int rowsAffected = dbCommand.ExecuteNonQuery();

            if (rowsAffected <= 0)
            {
                Console.WriteLine($"[Error] Package with ID {packageId} couldn't be removed from database!");
                return false;
            }

            return true;
        }
    }
}
