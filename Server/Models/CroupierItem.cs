using System;
using System.Collections.Generic;

namespace Casino.Server.Models
{
    public class Croupier : Player
    {
        // Krupier drzi herne balicky kariet
        public List<Deck> Decks { get; set; }
    }
}
