using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Casino.Menu
{
    public class ListMenu
    {
        // Viditelny zoznam zvonku triendy
        public List<MenuItem> Items { get; set; }

        private int SelectedIndex { get; set; }

        // Vybrana polozka
        public MenuItem SelectedItem
        {
            get {
                return Items[SelectedIndex];
            }
        }

        public ListMenu()
        {
            this.Items = new List<MenuItem>();
        }

        // Pridaj prvok do menu
        public void AddItem(MenuItem item)
        {
            if (!Items.Contains(item))
                Items.Add(item);
        }

        // Odstran prvok z menu
        public void DeleteItem(MenuItem item)
        {
            if (Items.Contains(item))
                Items.Remove(item);
        }

        public async Task<MenuItem> ResultAsync()
        {
            return await Task.FromResult<MenuItem>(this.Result());
        }

        // Vykona uzivatelom zvolenu akciu
        public Task InvokeResult()
        {
            return Task.Run(() =>
            {
                this.Result();
                this.SelectedItem.Action();
            });
        }

        // Vyber z menu
        public MenuItem Result()
        {
            Console.WriteLine("===============================================");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("                       Menu                    ");
            Console.ResetColor();
            Console.WriteLine("===============================================");

            // Odsadenie poloziek menu od laveho okraja okna
            int? alignmentLeft = null;

            // Povodna vertikalna poloha kurzora
            var originalTop = Console.CursorTop;

            // Nastav vyber podla kurzora
            SelectedIndex = Items.Count - 1;

            // Vypis poloziek menu
            for (int i = 0; i < Items.Count; i++)
            {
                if (i != SelectedIndex)
                {
                    // Vypis
                    alignmentLeft = this.Print(i, false, alignmentLeft);
                    // Vlozi novy riadok ked nasleduje dalsia polozka inak vrati kurzor na zaciatok
                    Console.WriteLine();
                }
                else
                {
                    // Vypis
                    alignmentLeft = this.Print(i, true, alignmentLeft);
                    // Vrati kurzor na zaciatok riadka
                    Console.CursorLeft = 0;
                }
            }

            // Vyber zo zoznamu
            while (true)
            {
                // Cakaj na stlacenie klavesy
                var key = Console.ReadKey(true).Key;

                // Urci o ktoru klavesu islo
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        {
                            if (SelectedIndex != 0)
                            {
                                alignmentLeft = this.Print(SelectedIndex, false, alignmentLeft);

                                SelectedIndex--;

                                Console.SetCursorPosition(0, (originalTop + SelectedIndex));
                                alignmentLeft = this.Print(SelectedIndex, true, alignmentLeft);
                                Console.CursorLeft = 0;
                            }
                            else
                                Console.Beep();

                            break;
                        }
                    case ConsoleKey.DownArrow:
                        {
                            if (SelectedIndex != (Items.Count - 1))
                            {
                                alignmentLeft = this.Print(SelectedIndex, false, alignmentLeft);

                                SelectedIndex++;

                                Console.SetCursorPosition(0, (originalTop + SelectedIndex));
                                alignmentLeft = this.Print(SelectedIndex, true, alignmentLeft);
                                Console.CursorLeft = 0;
                            }
                            else
                                Console.Beep();

                            break;
                        }
                    case ConsoleKey.Enter:
                        {
                            if (SelectedItem.IsEnabled)
                            {
                                Console.Clear();
                                return SelectedItem;
                            }
                            break;
                        }
                    default:
                        {
                            for (int i = 0; i < Items.Count; i++)
                            {
                                if (key == 0)
                                    continue;

                                if (key == Items[i].Key && Items[i].IsEnabled)
                                {
                                    SelectedIndex = i;
                                    Console.Clear();
                                    return SelectedItem;
                                }
                            }
                            Console.Beep();
                            break;
                        }
                }
            }
        }

        // Vypis polozky
        private int Print(int index, bool isSelected, int? alignmentLeft)
        {
            if (Items[index].Key != 0)
                Console.Write($" ({Items[index].Key.ToString()})\t\t");
            else
                Console.Write("\t\t");

            if (alignmentLeft.HasValue)
                Console.CursorLeft = alignmentLeft.Value;
            else
                alignmentLeft = Console.CursorLeft;

            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"{Items[index].Text}");
                Console.ResetColor();
            }
            else if (!Items[index].IsEnabled)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{Items[index].Text}");
                Console.ResetColor();
            }
            else
                Console.Write($"{Items[index].Text}");

            return alignmentLeft.Value;
        }
    }
}
