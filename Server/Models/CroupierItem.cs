using System;

namespace Casino.Server.Models
{
    public class Croupier : Player
    {
        // Krupier drzi herne balicky kariet
        public Deck Decks { get; set; }
    }
}
