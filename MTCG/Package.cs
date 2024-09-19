namespace MTCG
{
    internal class Package
    {
        private Card[] cards; // A package consists of 5 cards
        private int price = 5; // The price of a package is 5 coins

        // Constructor
        public Package()
        {
            // Fill package with 5 cards
            cards = new Card[5];
        }
    }
}