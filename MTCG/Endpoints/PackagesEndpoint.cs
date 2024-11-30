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
        private readonly AuthService _authService = AuthService.Instance;

        public (int, string?) HandleRequest(HTTPHeader headers, string body)
        {
            // Add new card
            if (headers.Method == "POST")
            {
                // Check if user is authorized to add cards
                if (!CheckAuthorization(headers))
                {
                    return (403, "User not authorized!");
                }

                // Each request adds an array of cards
                List<MonsterCard>? cardsToAdd = JsonSerializer.Deserialize<List<MonsterCard>>(body);

                // Check if request was not empty
                if (cardsToAdd != null)
                {
                    int numberOfCards = cardsToAdd.Count();
                    int cardsAdded = 0;

                    // Add cards to database
                    foreach (var card in cardsToAdd)
                    {
                        if (_cardRepository.AddCard(card))
                        {
                            cardsAdded++;
                        }
                    }

                    // Response
                    if (cardsAdded == numberOfCards)
                    {
                        return (201, "All cards added successfully");
                    } else if (cardsAdded < numberOfCards && cardsAdded != 0)
                    {
                        // This mapping is not entirely correct
                        return (206, "Some cards could not be added");
                    }
                    else
                    {
                        return (500, "Error adding card");
                    }
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
