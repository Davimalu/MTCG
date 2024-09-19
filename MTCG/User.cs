﻿namespace MTCG
{
    internal class User
    {
        private string username;
        private string password;
        private int elo;
        private int coinCount;

        private Stack stack;
        private Deck deck;

        // Constructor
        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.elo = 0;
            this.coinCount = 20;

            stack = new Stack();
            deck = new Deck();
        }
    }
}