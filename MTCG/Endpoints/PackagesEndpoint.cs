using MTCG.Logic;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Interfaces;
using MTCG.Repository;

namespace MTCG.Endpoints
{
    public class PackagesEndpoint : IHttpEndpoint
    {
        private readonly CardRepository _cardRepository = CardRepository.Instance;
        private readonly PackageRepository _packageRepository = PackageRepository.Instance;
        private readonly AuthService _authService = AuthService.Instance;
        private readonly PackageService _packageService = PackageService.Instance;

        public (int, string?) HandleRequest(HTTPHeader headers, string body)
        {
            // Add new package
            if (headers.Method == "POST")
            {
                // Check if user is authorized to add a package
                if (!CheckAuthorization(headers))
                {
                    return (403, "User not authorized!");
                }

                // Each request contains an array of cards
                List<MonsterCard>? cardsToAdd = JsonSerializer.Deserialize<List<MonsterCard>>(body);

                // Check if request was not empty
                if (cardsToAdd != null)
                {
                    int numberOfCards = cardsToAdd.Count();
                    int cardsAdded = 0;

                    // Temporary package data structure used to save package into database
                    Package tmpPackage = new Package();

                    // Iterate over all cards
                    foreach (var card in cardsToAdd)
                    {
                        // Add cards to database and temporary package
                        if (_cardRepository.AddCard(card) && _packageService.AddCardToPackage(card, tmpPackage))
                        {
                            cardsAdded++;
                        }
                    }

                    // Check if all cards were successfully added
                    if (cardsAdded != numberOfCards)
                    {
                        return (500, "Error adding package");
                    }

                    // Add package to database
                    if (!_packageRepository.AddPackage(tmpPackage))
                    {
                        return (500, "Error adding package");
                    }

                    return (201, "Package created successfully!");
                }

                return (400, "Invalid Request");
            }

            return (400, "Invalid Request");
        }

        private bool CheckAuthorization(HTTPHeader headers)
        {
            // Provided string should be something like "Bearer admin-mtcgToken"

            // Check for correct number of words in string
            string tmp = headers.Headers["Authorization"];
            Console.WriteLine($"[DEBUG] Authorization:{tmp}");
            if (headers.Headers["Authorization"].Split(' ').Length != 2)
            {
                return false;
            }

            // Check token against database
            return _authService.CheckToken(headers.Headers["Authorization"].Split(' ')[1]);

        }
    }
}
