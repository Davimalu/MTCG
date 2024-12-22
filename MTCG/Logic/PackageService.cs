using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Models;
using MTCG.Models.Cards;
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
        #region DependencyInjection
        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }
        #endregion

        public PackageService() { }

        private readonly IPackageRepository _packageRepository = PackageRepository.Instance;


        public bool AddCardToPackage(Card card, Package package)
        {
            // Thread Safety: Ensure that no other thread adds a card to the package between checking if the package is already full and the actual adding of the card
            lock (ThreadSync.CardLock)
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
        }


        public bool SavePackageToDatabase(Package package)
        {
            lock (ThreadSync.PackageLock)
            {
                return _packageRepository.AddPackageToDatabase(package);
            }
        }


        public Package? GetRandomPackage()
        {
            // Thread Safety: Ensure that no other thread tries to access (and in succession delete) the same package at the same time
            lock (ThreadSync.PackageLock)
            {
                // Get random package from database
                int? randomPackageId = _packageRepository.GetIdOfRandomPackage();
                if (randomPackageId == null)
                {
                    return null;
                }

                Package? tmpPackage = _packageRepository.GetPackageById((int)randomPackageId);
                if (tmpPackage == null)
                {
                    return null;
                }

                // Delete retrieved package from database
                _packageRepository.DeletePackageById((int)randomPackageId);

                return tmpPackage;
            }
        }
    }
}
