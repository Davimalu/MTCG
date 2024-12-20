using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Interfaces;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class PackageService : IPackageService
    {
        #region Singleton
        private static PackageService? _instance;

        public static PackageService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PackageService();
                }

                return _instance;
            }
        }
        #endregion

        private readonly PackageRepository _packageRepository = PackageRepository.Instance;

        public bool AddCardToPackage(Card card, Package package)
        {
            // Check if package is already full
            if (package.Cards.Count() >= 5)
            {
                return false;
            }

            // Add card to package
            package.Cards.Add(card);
            return true;
        }

        public bool SavePackageToDatabase(Package package)
        {
            return _packageRepository.AddPackageToDatabase(package);
        }

        public Package? GetRandomPackage()
        {
            // TODO: THREAD SAFETY

            // Get random package from database
            int randomPackageId = _packageRepository.GetRandomPackageId();
            Package? tmpPackage = _packageRepository.GetPackageFromId(randomPackageId);

            // Delete retrieved package from database
            _packageRepository.DeletePackageById(randomPackageId);

            return tmpPackage;
        }
    }
}
