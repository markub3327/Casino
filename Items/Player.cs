using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Casino.Items
{
    public class Player
    {
        // Meno hraca (prezyvka)
        [Key]
        public string Nickname { get; set; }
        // Penazeka
        public long Wallet { get; set; }
        // Vyska stavky hraca
        public long Bet { get; set; }
        // Karty na ruke
        public IEnumerable<Card> Cards { get; set; }
        // Hracova hra
        public string GameId { get; set; }
        // Token hraca
        public string Token { get; set; }
        // Akcia hraca
        public EActions Action { get; set; }
        // Stav hraca
        public EState State { get; set; }
        // Sucet kariet
        public int CardSum
        {
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


        public enum EActions
        {
            NONE,
            HIT,
            STAND,
            DOUBLE,
            EXIT
        }

        public enum EState
        {
            FREE,
            PLAYING,
            WIN,
            LOSE,
            DRAW
        }
    }
}
