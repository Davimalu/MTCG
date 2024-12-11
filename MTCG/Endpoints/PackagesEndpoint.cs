﻿using MTCG.Logic;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Repository;

namespace MTCG.Endpoints
{
    public class PackagesEndpoint : IHttpEndpoint
    {
        private readonly CardRepository _cardRepository = CardRepository.Instance;
        private readonly PackageRepository _packageRepository = PackageRepository.Instance;
        private readonly UserService _userService = UserService.Instance;
        private readonly PackageService _packageService = PackageService.Instance;
        private readonly IHeaderHelper _headerHelper = new HeaderHelper();

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _headerHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            // Add new package
            if (headers.Method == "POST")
            {
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
                        if (_cardRepository.AddCardToDatabase(card) && _packageService.AddCardToPackage(card, tmpPackage))
                        {
                            cardsAdded++;
                        }
                    }

                    // Check if all cards were successfully added
                    if (cardsAdded != numberOfCards)
                    {
                        // TODO: Catch duplicate errors in repository and allow for packages to be added anyway if the card already exists in the database
                        return (500, JsonSerializer.Serialize("Error writing cards to database"));
                    }

                    // Add package to database
                    if (!_packageRepository.AddPackageToDatabase(tmpPackage))
                    {
                        return (500, JsonSerializer.Serialize("Error writing package to database"));
                    }

                    return (201, JsonSerializer.Serialize("Package created successfully"));
                }

                return (400, JsonSerializer.Serialize("Invalid card format used"));
            }

            return (405, JsonSerializer.Serialize("Method Not Allowed"));
        }
    }
}
