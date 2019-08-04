using System;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Game
    {
        // Jedinecny nazov hry
        [Key]
        public string Name { get; set; }
        // Popis hry
        public string Description { get; set; }
    }
}
