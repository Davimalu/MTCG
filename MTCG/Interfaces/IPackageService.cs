using MTCG.Models;

namespace MTCG.Interfaces;

public interface IPackageService
{
    bool AddCardToPackage(Card card, Package package);
    Package? GetRandomPackage();
}