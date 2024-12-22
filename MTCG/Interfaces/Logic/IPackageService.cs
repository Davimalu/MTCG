using MTCG.Models;
using MTCG.Models.Cards;

namespace MTCG.Interfaces.Logic;

public interface IPackageService
{
    /// <summary>
    /// adds a new card to a package
    /// </summary>
    /// <param name="card">card to be added to the package</param>
    /// <param name="package">package that the card should be added to</param>
    /// <returns>
    /// <para>true if the card was successfully added to the package</para>
    /// <para>false if the package was already full (=contained 5 or more cards)</para>
    /// </returns>
    bool AddCardToPackage(Card card, Package package);
    /// <summary>
    /// saves a new package (and all its cards) to the database
    /// </summary>
    /// <param name="package">package object containing all cards that belong to this package</param>
    /// <returns>
    ///<para>true if package was added successfully</para>
    ///<para>false if package couldn't be added to database</para>
    /// </returns>
    bool SavePackageToDatabase(Package package);
    /// <summary>
    /// retrieve a random Package from the database (and remove it from the database)
    /// </summary>
    /// <returns>
    /// <para>the package filled with all cards belonging to it on success</para>
    /// <para>null on error or if there are no packages available</para>
    /// </returns>
    Package? GetRandomPackage();
}