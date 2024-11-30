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
    }
}
