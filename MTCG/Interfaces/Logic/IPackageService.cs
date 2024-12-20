using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IPackageService
{
    bool AddCardToPackage(Card card, Package package);
    bool SavePackageToDatabase(Package package);
    Package? GetRandomPackage();
}