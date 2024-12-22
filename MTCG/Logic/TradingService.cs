using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Models;
using MTCG.Models.Enums;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class TradingService : ITradingService
    {
        #region Singleton
        private static TradingService? _instance;

        public static TradingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TradingService();
                }

                return _instance;
            }
        }
        #endregion
        #region DependencyInjection
        public TradingService(ITradeRepository tradeRepository, IUserService userService, ICardRepository cardRepository, IStackService stackService, IEventService eventService)
        {
            _tradeRepository = tradeRepository;
            _userService = userService;
            _cardRepository = cardRepository;
            _stackService = stackService;
            _eventService = eventService;
        }
        #endregion

        public TradingService() { }

        private readonly ITradeRepository _tradeRepository = TradeRepository.Instance;
        private readonly IUserService _userService = UserService.Instance;
        private readonly ICardRepository _cardRepository = CardRepository.Instance;
        private readonly IStackService _stackService = StackService.Instance;

        private readonly IEventService _eventService = new EventService();


        public TradeOffer? CreateTradeOffer(User user, Card card, bool requestedMonsterCard, float requestedDamage)
        {
            TradeOffer offer = new TradeOffer()
            {
                Id = null,
                User = user,
                Card = card,
                RequestedMonster = requestedMonsterCard,
                RequestedDamage = requestedDamage
            };

            // Get card from users' stack
            Card? cardToRemove = user.Stack.Cards.FirstOrDefault(item => item.Id == card.Id);
            if (cardToRemove == null)
            {
                // User doesn't own card
                _eventService.LogEvent(EventType.Warning, $"Couldn't create trade offer: User {user.Username} doesn't own Card {card.Name}", null);
                return null;
            }

            // Remove card from stack of user
            if (!_stackService.RemoveCardFromStack(cardToRemove, user.Stack))
            {
                // User doesn't own card
                _eventService.LogEvent(EventType.Warning, $"Couldn't create trade offer: User {user.Username} doesn't own Card {card.Name}", null);
                return null;
            }

            // Add trade offer to database
            offer.Id = _tradeRepository.AddTradeOfferToDatabase(offer);
            if (offer.Id == null)
            {
                _eventService.LogEvent(EventType.Error, $"Couldn't create trade offer: Database query failed", null);
                return null;
            }

            // Update user in database (card was removed from Stack)
            _userService.SaveUserToDatabase(user);

            // Now that the offer was created, see if there is another trade offer that is compatible with this one
            TryToTrade(offer);

            return offer;
        }


        public bool RemoveTradeOfferByCardId(string cardId)
        {
            TradeOffer? offerToRemove = GetTradeOfferByCardId(cardId);
            if (offerToRemove == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't remove trade offer for card {cardId}: There is no trade offer for this card", null);
                return false;
            }

            return RemoveTradeOffer(offerToRemove);
        }


        public bool RemoveTradeOffer(TradeOffer offerToRemove)
        {
            if (_tradeRepository.RemoveTradeDeal(offerToRemove))
            {
                // If the trade offer was successfully deleted, re-add card to stack of user
                _stackService.AddCardToStack(offerToRemove.Card, offerToRemove.User.Stack);
                _userService.SaveUserToDatabase(offerToRemove.User);

                return true;
            }

            return false;
        }


        public TradeOffer? GetTradeOfferByCardId(string cardId)
        {
            TradeOffer? offer = _tradeRepository.GetTradeDealByCardId(cardId);

            if (offer == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve trade offer for card {cardId}: There is no trade offer for this card", null);
                return null;
            }

            // Populate user and card fields
            if (!PopulateTradeOfferFields(offer))
            {
                return null;
            }

            return offer;
        }


        public TradeOffer? GetTradeOfferById(int tradeId)
        {
            TradeOffer? offer = _tradeRepository.GetTradeDealById(tradeId);

            if (offer == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve trade offer with ID {tradeId}: There is no trade offer with this ID", null);
                return null;
            }

            // Populate user and card fields
            if (!PopulateTradeOfferFields(offer))
            {
                return null;
            }

            return offer;
        }


        public List<TradeOffer> GetAllActiveTradeOffers()
        {
            List<TradeOffer> offers = _tradeRepository.GetAllTradeDeals();

            // Populate user and card fields of all results
            foreach (TradeOffer offer in offers.ToList())
            {
                if (!PopulateTradeOfferFields(offer))
                {
                    // Trade Offer is faulty and should be removed
                    RemoveTradeOffer(offer);
                    offers.Remove(offer);
                }
            }

            return offers;
        }


        /// <summary>
        /// populate the user and card field of the trade offer with instances of these objects containing all information stored about them
        /// </summary>
        /// <param name="offer">TradeOffer object containing the userID and cardID</param>
        /// <returns>
        /// <para>true if the TradeOffer object was successfully populated with the User and Card Information</para>
        /// <para>false on error, e.g. User and/or Card associated with the trade offer don't exist</para>
        /// </returns>
        private bool PopulateTradeOfferFields(TradeOffer offer)
        {
            // 
            // The errors in these error checks should be near impossible, but I guess it's good practice to check for them anyway
            User? offeringUser = _userService.GetUserById(offer.User.Id);
            Card? offeredCard = _cardRepository.GetCardById(offer.Card.Id);

            if (offeringUser == null)
            {
                _eventService.LogEvent(EventType.Error, $"Couldn't retrieve trade offer: The user associated with this trade offer doesn't exist", null);
                return false;
            }

            if (offeredCard == null)
            {
                _eventService.LogEvent(EventType.Error, $"Couldn't retrieve trade offer: The card associated with this trade offer doesn't exist", null);
                return false;
            }

            offer.User = offeringUser;
            offer.Card = offeredCard;
            return true;
        }


        public bool TryToTrade(TradeOffer newOffer)
        {
            // Ensure that no user information is changed while this function is executed - otherwise the User objects might contain outdated information by the time SaveUserToDatabase() is called
            lock (ThreadSync.UserLock)
            {
                List<TradeOffer> allOffers = GetAllActiveTradeOffers();

                // No other offers exist -> cannot trade
                if (allOffers.Count < 2)
                {
                    return false;
                }

                foreach (TradeOffer otherOffer in allOffers)
                {
                    // Skip combinations where the user would trade with themselves
                    if (otherOffer.User.Username == newOffer.User.Username || otherOffer.Id == null || newOffer.Id == null)
                    {
                        continue;
                    }

                    // Check if the card of the new offer is compatible with the requirements of existing offers
                    if ((newOffer.Card is MonsterCard && otherOffer.RequestedMonster == true) ||
                        (!(newOffer.Card is MonsterCard) && otherOffer.RequestedMonster == false))
                    {
                        if (newOffer.Card.Damage >= otherOffer.RequestedDamage)
                        {
                            // The other offer would accept this card
                            // Now check if the requirements of this offer would accept the card of the other offer
                            if ((otherOffer.Card is MonsterCard && newOffer.RequestedMonster == true) ||
                                !(otherOffer.Card is MonsterCard) && newOffer.RequestedMonster == false)
                            {
                                if (otherOffer.Card.Damage >= newOffer.RequestedDamage)
                                {
                                    // Both trade offers are compatible, commence trade!
                                    _stackService.AddCardToStack(newOffer.Card, otherOffer.User.Stack);
                                    _stackService.AddCardToStack(otherOffer.Card, newOffer.User.Stack);

                                    _userService.SaveUserToDatabase(otherOffer.User);
                                    _userService.SaveUserToDatabase(newOffer.User);

                                    _tradeRepository.SetTradeOfferInactive((int)otherOffer.Id);
                                    _tradeRepository.SetTradeOfferInactive((int)newOffer.Id);

                                    _eventService.LogEvent(EventType.Highlight, $"{newOffer.User.Username} received card {otherOffer.Card.Name} in trade with {otherOffer.User.Username}", null);
                                    _eventService.LogEvent(EventType.Highlight, $"{otherOffer.User.Username} received card {newOffer.Card.Name} in trade with {newOffer.User.Username}", null);

                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
