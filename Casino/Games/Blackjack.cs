using System;
using System.Collections.Generic;

namespace Casino.Games
{
    public class Blackjack
    {
        // Multiplayer
        private Client.Client client;

        // Trieda hry Blackjack - herny stol
        public Blackjack(Client.ServerInfo serverInfo)
        {
            this.client = new Client.Client(serverInfo);
        }

        // Nakresli herny stol do konzoly
        public void Print(List<Items.Player> players)
        {

        }
    }
}
