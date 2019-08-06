using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Player
    {
        // Meno hraca (nickname)
        [Key]
        public string Name { get; set; }
        // Penazeka
        public long Wallet { get; set; }
        // Vyska stavky hraca
        public long Bet { get; set; }
        // Karty na ruke
        public IEnumerable<Card> Cards { get; set; }
        // ID hry, ktoru prave hrac hra
        public string GameId { get; set; }
        // ID akcie, ktoru prave hrac vykonal
        public string ActionId { get; set; }
        // Token
        public string Token { get; set; }
        // Stav hraca
        public EState State { get; set; }


        // Sucet kariet
        public int CardSum {
            get
            {
                if (this.Cards != null)
                {
                    int CardSumCounter = 0; 

                    foreach (var c in Cards)
                    {
                        switch (c.Value)
                        {
                            case "J":
                            case "Q":
                            case "K":
                                {
                                    CardSumCounter += 10;
                                    break;
                                }
                            case "A":
                                {
                                    var x = CardSumCounter + 11;

                                    if (x <= 21)
                                        CardSumCounter = x;
                                    else
                                        CardSumCounter += 1;

                                    break;
                                }
                            default:
                                if (int.TryParse(c.Value, out int val))
                                    CardSumCounter += val;
                                break;

                        } 
                    }

                    return CardSumCounter;
                }
                return -1;
            }
        }

        // Vypis udajov o hracovi na konzolu
        public virtual void Print()
        {
            Console.WriteLine($"Player {Name}:");
            Console.WriteLine($"\tWallet: {Wallet}");
            Console.WriteLine($"\tBet: {Bet}");
            Console.WriteLine($"\tPlaying: {GameId}");
            Console.WriteLine($"\tAction: {ActionId}");
            Console.WriteLine($"\tState: {State.ToString()}");
            Console.WriteLine($"\tToken: {Uri.EscapeDataString(Token)}\n");
            if (Cards != null)
            {
                Console.Write("\t");
                foreach (var card in Cards)
                {
                    card.Print();
                    Console.Write("\t");
                }
                Console.WriteLine("\n");
            }
        }

        // Stavy v akych sa hrac moze nachadzat
        public enum EState
        {
            None,       // Neaktivny
            Playing,    // Hrajuci hru
            Win,        // Vytaz
            Lose,       // Prehral
            Draw        // Remiza    
        }
    }
}
