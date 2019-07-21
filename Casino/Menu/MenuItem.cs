using System;
namespace Casino.Menu
{
    public class MenuItem
    {
        // Text polozky
        public string Text { get; set; }
        // Akcia
        public Action Action { get; set; }
        // Je polozka v menu povolena
        public bool IsEnabled { get; set; }
        // Klavesa
        public ConsoleKey Key { get; set; }
    }
}
