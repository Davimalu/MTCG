using MTCG.Models;

namespace MTCG.Interfaces.Repository;

public interface IPackageRepository
{
    /// <summary>
    /// saves a new package to the database
    /// </summary>
    /// <param name="package">package object containing all cards that belong to this package</param>
    /// <returns>
    ///<para>true if package was added successfully</para>
    ///<para>false if package couldn't be added to database</para>
    /// </returns>
    bool AddPackageToDatabase(Package package);

    /// <summary>
    /// saves the information on which cards belong to this package in the database
    /// </summary>
    /// <param name="package">package object containing the packageId of the database entry in table 'packages' and all cards that belong to this package</param>
    /// <returns>
    ///<para>true if all associations were successfully saved to the database</para>
    /// <para>false if at least one association couldn't be saved in the database</para>
    /// </returns>
    bool AddCardsToPackage(Package package);

    /// <summary>
    /// gets the packageId of a random package from the packages table
    /// </summary>
    /// <returns>
    /// <para>packageId of a random package on success</para>
    /// <para>0 if no packages available</para>
    /// </returns>
    int GetRandomPackageId();

    /// <summary>
    /// retrieves a package from the database using its packageId
    /// </summary>
    /// <param name="packageId"></param>
    /// <returns>
    /// <para>package containing all metadata and associated cards on success</para>
    /// <para>null if the package doesn't exist or has no cards associated with it</para>
    /// </returns>
    Package? GetPackageFromId(int packageId);

    /// <summary>
    /// delete a package from the database using its packageId; does NOT delete the cards that were added with that package
    /// </summary>
    /// <param name="packageId"></param>
    /// <returns>
    /// <para>true if package was deleted successfully</para>
    /// <para>false if there was no package with that packageId or if SQL error occured</para>
    /// </returns>
    bool DeletePackageById(int packageId);
}