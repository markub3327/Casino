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

        private string Title;

        // Vybrana polozka
        public MenuItem SelectedItem
        {
            get {
                return Items[SelectedIndex];
            }
        }

        public ListMenu(string title = "Menu")
        {
            this.Title = title;
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
      
        // Vykona uzivatelom zvolenu akciu
        public Task InvokeResult()
        {
            return Task.Run(() =>
            {
                this.Print();
                if (this.SelectedItem != null)
                    this.SelectedItem.Action();
            });
        }

        // Vyber z menu
        public void Print()
        {
            // Vzor nadpisu
            var titleS = $"                        {Title}                        ";

            for (int i = 0; i < titleS.Length; i++)
                Console.Write("=");
            Console.WriteLine();

            // Nadpis
            //Console.CursorLeft = (titleS.Length - Title.Length);
            Program.ShowError(titleS);
            Console.WriteLine();

            for (int i = 0; i < titleS.Length; i++)
                Console.Write("=");
            Console.WriteLine();

            // Odsadenie poloziek menu od laveho okraja okna
            int? alignmentLeft = null;

            // Nastav vyber podla kurzora
            SelectedIndex = Items.Count - 1;

            // Vypis poloziek menu
            for (int i = 0; i < Items.Count; i++)
            {
                // Vypis
                this.Print(i, (i == SelectedIndex), ref alignmentLeft);
                // Vlozi novy riadok ked nasleduje dalsia polozka
                Console.WriteLine();
            }

            // Nastav kurzor na poslednu polozku
            Console.SetCursorPosition(0, (Console.CursorTop - 1));

            // Zneviditelni kurzor
            Console.CursorVisible = false;

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
                                this.Print(SelectedIndex, false, ref alignmentLeft);

                                SelectedIndex--;

                                Console.SetCursorPosition(0, (Console.CursorTop - 1));
                                this.Print(SelectedIndex, true, ref alignmentLeft);
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
                                this.Print(SelectedIndex, false, ref alignmentLeft);

                                SelectedIndex++;

                                Console.SetCursorPosition(0, (Console.CursorTop + 1));
                                this.Print(SelectedIndex, true, ref alignmentLeft);
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
                                // Vycisti konzolu
                                Console.Clear();
                                // Zviditelni kurzor
                                Console.CursorVisible = true;
                                // Koniec vyberu
                                return;
                            }
                            break;
                        }
                    default:
                        {
                            for (int i = 0; i < Items.Count; i++)
                            {
                                if (key == Items[i].Key && Items[i].IsEnabled)
                                {
                                    // Uloz index vybranej polozky
                                    SelectedIndex = i;
                                    // Vycisti konzolu
                                    Console.Clear();
                                    // Zviditelni kurzor
                                    Console.CursorVisible = true;
                                    // Koniec vyberu
                                    return;
                                }
                            }
                            Console.Beep();
                            break;
                        }
                }
            }
        }

        // Vypis polozky
        private void Print(int index, bool isSelected, ref int? alignmentLeft)
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
        }
    }
}
