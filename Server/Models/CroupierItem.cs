using System;
using System.Collections.Generic;

namespace Casino.Server.Models
{
    public class Croupier : Items.Player
    {
        // Krupier drzi herne balicky kariet
        public List<Deck> Decks { get; set; }

        public int CardSum(long playerId, ApiContext context)
        {
            return 0;
        }
    }
}
