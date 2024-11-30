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

        public (int, string?) HandleRequest(string method, string body)
        {
            // Add new card
            if (method == "POST")
            {
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
    }
}
