﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Endpoints
{
    public class DeckEndpoint : IHttpEndpoint
    {
        private readonly CardRepository _cardRepository = CardRepository.Instance;
        private readonly UserService _userService = UserService.Instance;
        private readonly DeckService _deckService = DeckService.Instance;

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

            // GET => List deck of user
            if (headers.Method == "GET")
            {
                // Check for query Parameters
                Dictionary<string, string> queryParameters = _headerHelper.GetQueryParameters(headers);

                // https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.dictionary-2.trygetvalue?view=net-8.0
                if (queryParameters.TryGetValue("format", out string? format))
                {
                    // Query Parameter format present
                    if (format == "plain")
                    {
                        // TODO: HTTP Header application/json is wrong
                        string plainText = _deckService.SerializeDeckToPlaintext(user.Deck);
                        return (200, plainText);
                    }
                    else
                    {
                        return (400, JsonSerializer.Serialize("Invalid query parameter"));
                    }
                }
                else // No Query Parameters
                {
                    // Convert user deck into JSON
                    string json = JsonSerializer.Serialize(user.Deck.Cards);

                    return (200, json);
                }
            }

            // PUT => Update deck of user
            if (headers.Method == "PUT")
            {
                List<Card>? configuredDeck = ProcessDeckConfiguration(user, body);

                if (configuredDeck == null)
                {
                    return (400, JsonSerializer.Serialize("Invalid deck configuration provided"));
                }

                user.Deck.Cards = configuredDeck;

                // Save changes to database
                _userService.SaveUserToDatabase(user);

                return (200, JsonSerializer.Serialize("Deck updated"));
            }

            return (405, JsonSerializer.Serialize("Method Not Allowed"));
        }

        /// <summary>
        /// parses 
        /// </summary>
        /// <param name="user">user object of the user whose deck should be configured</param>
        /// <param name="body">the body of the request</param>
        /// <returns>
        /// <para>returns a list of cards representing the deck configuration that was chosen by the user on success</para>
        /// <para>returns null if the request format was invalid or the user doesn't own the cards</para>
        /// </returns>
        private List<Card>? ProcessDeckConfiguration(User user, string body)
        {
            // TODO: Refactor into multiple functions
            // Check if Deck configuration is valid
            List<string>? cardIds;
            try
            {
                cardIds = JsonSerializer.Deserialize<List<string>>(body);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"[WARNING] User provided invalid payload!");
                Console.WriteLine($"[WARNING] {ex.Message}");
                Console.ResetColor();
                return null;
            }

            // Check if exactly 4 cards were provided
            if (cardIds == null || cardIds.Count != 4)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"[WARNING] User provided invalid deck configuration!");
                Console.ResetColor();
                return null;
            }

            List<Card> configuredDeck = new List<Card>(4);

            // For each card in the configuration provided...
            foreach (string cardId in cardIds)
            {
                Card? card = _cardRepository.GetCardById(cardId);

                // Card doesn't exist
                if (card == null)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"[WARNING] User tried to configure his deck with a card that doesn't exist!");
                    Console.ResetColor();
                    return null;
                }
                
                // Check if user owns these cards (= user has these cards in his stack)
                if (user.Stack.Cards.All(stackCard => stackCard.Id != card.Id)) // LINQ Expression
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"[WARNING] User tried to configure his deck with a card he doesn't own!");
                    Console.ResetColor();
                    return null;
                }

                configuredDeck.Add(card);
            }

            return configuredDeck;
        }
    }
}
