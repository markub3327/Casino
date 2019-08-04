using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino.Server.Controllers
{
    public class BlackjackController : ControllerBase
    {
        private readonly Models.ApiContext _context;

        private Models.Croupier _croupier;

        public enum Actions
        {
            Hit,
            Stand,
            Double,
            Split,
            Exit
        }

        public BlackjackController(Models.ApiContext context)
        {
            _context = context;

            // Ak v Db nie su akcie hracov, vytvor zaznamy
            if (_context.Actions.Count() == 0)
            {
                // Pridaj hru do Db
                _context.Actions.Add(new Items.Action { Name = Actions.Hit.ToString(), Description = "" });          // Zobrat od krupiera dalsiu kartu.
                _context.Actions.Add(new Items.Action { Name = Actions.Stand.ToString(), Description = "" });        // Zostat stat, ukoncit hru. Hrac si neberie dalsiu kartu,
                _context.Actions.Add(new Items.Action { Name = Actions.Double.ToString(), Description = "" });       // Zdvojnasobit vsadenu ciastku. Dostane este jednu kartu a hra konci.
                _context.Actions.Add(new Items.Action { Name = Actions.Split.ToString(), Description = "" });        // Rozdelit hru. Iba ak ma hrac dve karty rovnakej hodnoty sa hra rozdeli tak,
                                                                                                                     //  ze s kazdou kartou sa hra samostatna hra         
                _context.Actions.Add(new Items.Action { Name = Actions.Exit.ToString(), Description = "" });
                _context.SaveChanges();
            }

            var standPlayers = _context.Players.Where(p =>
                (p.ActionId == "Stand" && p.Name != "Croupier-Blackjack" && p.GameId == "Blackjack" && p.State == Items.Player.EState.Playing));

            var players = _context.Players.Where(p =>
                (p.GameId == "Blackjack" && p.Name != "Croupier-Blackjack" && p.State == Items.Player.EState.Playing))
                .Include(p => p.Cards);

            // Pridaj krupiera ak neexistuje
            AddCroupier();

            // Ak su hraci
            if (players.Count() > 0)
            {
                // Ak vsetci hraci skoncili hru, hra krupier
                if (standPlayers.Count() == players.Count())
                {
                    do
                    {
                        _croupier.Print();

                        // Hra krupiera
                        var Csum = _croupier.CardSum;
                        Console.WriteLine($"_croupier.sum = {Csum}");
                        if (Csum >= 17) break;

                        // Potiahni kartu
                        var deck = _context.Decks.First();
                        deck.GetCard(_croupier.Name, _context).Wait();

                        // Uloz do Db
                        _context.SaveChanges();
                    } while (true);

                    // Vyhodnotenie hry
                    foreach (var p in players)
                    {
                        var res = Rules(p);

                        Console.WriteLine($"Result for Player {p.Name}: {res}");

                        // Vyhra
                        if (res == 1)
                            Win(p);
                        // Prehra
                        else if (res == -1)
                            Lose(p);
                        // Remiza
                        else
                            Draw(p);
                    }

                    // Uvolni karty
                    foreach (var c in _croupier.Cards)
                    {
                        c.PlayerId = null;
                    }

                    _context.SaveChanges();
                }
            }
        }

        // GET casino/blackjack
        [HttpGet("casino/{controller}")]
        public async Task<IActionResult> Index()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Casino.Server.blackjack.html"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = StatusCodes.Status200OK,
                        Content = await reader.ReadToEndAsync()
                    };
                }
            }
        }

        // GET casino/blackjack/actions
        // GET casino/blackjack/actions?token=XXX
        [HttpGet("casino/{controller}/actions"), Produces("application/json")]
        public async Task<IActionResult> GetActions([FromQuery(Name = "name")]string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var actions = await _context.Actions.ToListAsync();

                return Ok(actions.Select(a => new
                {
                    name = a.Name
                }));
            }
            else
            {
                var actions = await _context.Actions.Where(g => (g.Name == name)).ToListAsync();

                if (actions.Count > 0)
                    return Ok(actions[0]);

                return NotFound();
            }
        }

        // GET casino/blackjack/players
        // GET casino/blackjack/players?token=XXX
        [HttpGet("casino/{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> GetPlayers([FromQuery(Name = "token")]string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var players = await _context.Players.Where(p =>
                    (p.Token == token && p.GameId == "Blackjack"))
                    .Include(p => p.Cards).ToListAsync();

                // Ak existuje hrac (s tokenom)
                if (players.Count > 0)
                {                   
                    return Ok(players[0]);
                }
                return NotFound();
            }
            else
            {
                // Vytvor pole hracov pri stole obsahujuce aj ich karty
                var players = await _context.Players.Where(p => (p.GameId == "Blackjack"))
                    .Include(p => p.Cards).ToListAsync();

                // Vytvor vystup JSON zo zoznamu hracov pri stole
                return Ok(players.Select(p => new
                {
                    name = p.Name,
                    bet = p.Bet,
                    cards = p.Cards.Select(c => new { suit = c.Suit, value = c.Value }),
                    state = p.State,
                    gameid = p.GameId,
                    actionid = p.ActionId
                }));
            }
        }

        // POST casino/players
        [HttpPost("casino/{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> PostPlayer([FromBody]Items.Player player)
        {
            // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
            var players = await _context.Players.Where(p =>
                (p.Token == player.Token && p.State != Items.Player.EState.Playing))
                .Include(p => p.Cards).ToListAsync();

            // Ak existuje v Db hrac (s tokenom a nehra ziadnu hru)
            if (players.Count > 0)
            {
                // Nastav aktivnu hru hraca
                players[0].GameId = "Blackjack";

                // Nastav stav hraca na hrajuceho hru
                players[0].State = Items.Player.EState.Playing;

                // Vynuluj akcie
                players[0].ActionId = null;

                // Aktualizuje sa vyska stavky, ktoru ovplyvnuje hrac pocas hry
                // Minimalna stavka je 100$
                if (player.Bet < 100) players[0].Bet = 100;
                else players[0].Bet = player.Bet;

                // Aktualizuj stav penazenky
                players[0].Wallet = player.Wallet;

                // Vezmi karty
                await GivingCards(players[0]);

                // Uloz zmeny profilu hraca do Db
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlayers), new { token = players[0].Token }, players[0]);
            }

            return BadRequest();
        }

        // PUT casino/blackjack/player?token=XXX
        [HttpPut("casino/{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> PutPlayer([FromQuery(Name = "token")]string token, [FromBody]Items.Player player)
        {
            if (player != null)
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var players = await _context.Players.Where(p =>
                    (p.Token == token && p.GameId == "Blackjack" && p.State == Items.Player.EState.Playing))
                    .Include(p => p.Cards).ToListAsync();

                // Ak existuje v Db hrac (s tokenom)
                if (players.Count > 0)
                {
                    // Aktualizuj akciu hraca
                    players[0].ActionId = player.ActionId;

                    // ... vykona akcie
                    switch (players[0].ActionId)
                    {
                        case "Hit":
                            {
                                var deck = _context.Decks.First();
                                await deck.GetCard(players[0].Name, _context);
                                await _context.SaveChangesAsync();

                                // skontroluj ci neni cez 21 alebo prave 21
                                var sum = players[0].CardSum;
                                if (sum > 21)
                                {
                                    Lose(players[0]);
                                }
                                else if (sum == 21)
                                {
                                    players[0].ActionId = "Stand";
                                }

                                break;
                            }
                        case "Stand":
                            {
                                Console.WriteLine($"Player {players[0].Name} end the game.");
                                break;
                            }
                        case "Double":
                            {
                                var deck = _context.Decks.First();
                                await deck.GetCard(players[0].Name, _context);
                                players[0].Bet <<= 1;
                                // ... potom konci hru ...
                                players[0].ActionId = "Stand";
                                break;
                            }
                        case "Exit":
                            {
                                _croupier.Wallet += players[0].Bet >> 1;
                                players[0].Wallet -= players[0].Bet >> 1;
                                players[0].Bet = 0;
                                players[0].ActionId = null;
                                players[0].GameId = null;
                                foreach (var c in players[0].Cards) // Uvloni karty
                                {
                                    c.PlayerId = null;
                                }
                                break;
                            }
                    }

                    // Uloz zmeny profilu hraca do Db
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }

            return BadRequest();
        }

        // Vyhra
        private void Win(Items.Player player)
        {
            _croupier.Wallet -= player.Bet;
            player.Wallet += player.Bet;
            player.Bet = 0;
            player.ActionId = null;
            player.State = Items.Player.EState.Win;
            foreach (var c in player.Cards)
            {
                c.PlayerId = null;
            }
        }

        // Prehra
        private void Lose(Items.Player player)
        {
            _croupier.Wallet += player.Bet;
            player.Wallet -= player.Bet;
            player.Bet = 0;
            player.ActionId = null;
            player.State = Items.Player.EState.Lose;
            foreach (var c in player.Cards)
            {
                c.PlayerId = null;
            }
        }

        // Remiza - vrati stavku
        private void Draw(Items.Player player)
        {
            player.Bet = 0;
            player.ActionId = null;
            player.State = Items.Player.EState.Draw;
            foreach (var c in player.Cards)
            {
                c.PlayerId = null;
            }
        }

        // Pravidla hry Blackjack
        private int Rules(Items.Player player)
        {
            var sum = player.CardSum;
            var Csum = _croupier.CardSum;

            // Hrac prehral - koniec hry ak prekrocil 21
            if (sum > 21)
            {
                return -1;
            }
            // Hrac vyhral - koniec hry ak krupier prekrocil 21 a hrac nie
            // Ak ma hrac vyssi sucet kariet ako krupier
            if ((Csum > 21) || (sum > Csum))
            {
                return 1;
            }            
            // Ak ma rovnaky sucet ako krupier vrati sa mu stavka
            if (sum == Csum)
            {
                return 0;
            }

            // Inak prehra
            return -1;
        }

        private void AddCroupier()
        {
            var croupier = _context.Croupiers.Where(p =>
                (p.Name == "Croupier-Blackjack"))
                .Include(p => p.Cards);

            // Zisti ci uz je vytvoreny krupier hry v Db
            if (!croupier.Any())
            {
                // Vytvor objekt krupiera
                _croupier = new Models.Croupier
                {
                    Name = "Croupier-Blackjack",
                    Wallet = 1000000,   // krupier ma bank (kasino zacina s 1.000.000$/krupier existenciu)
                    GameId = "Blackjack",
                    State = Items.Player.EState.Playing
                };

                // Vygeneruj token pre komunikaciu
                TokenGenerator.Generate(_croupier);

                // Pridaj krupiera do Db
                _context.Croupiers.Add(_croupier);
                _context.SaveChanges();

                // Vygeneruj herne balicky kariet
                // Id v Db sa pocita od 1
                //for (int i = 1; i <= 1; i++)
                //{
                // Vytvor objekt balicka
                var deck = new Models.Deck { CroupierId = _croupier.Name };

                // Pridaj balicek do Db
                _context.Decks.Add(deck);
                _context.SaveChanges();

                // Vygeneruj karty
                deck.Create(_context);
                _context.SaveChanges();
                //}

                Console.WriteLine($"DeckId: {deck.Id}");

                // Krupier zacina s jednou kartou
                deck.GetCard(_croupier.Name, _context).Wait();
                _context.SaveChanges();
            }
            else
            {
                // Obnov krupiera pre dalsie hry
                _croupier = croupier.First();

                if (!_croupier.Cards.Any())
                {
                    // Krupier zacina s jednou kartou
                    var deck = _context.Decks.First();
                    deck.GetCard(_croupier.Name, _context).Wait();
                    _context.SaveChanges();
                }
            }
        }

        private async Task GivingCards(Items.Player player)
        {
            // Novy hrac ak este nedostal karty
            var deck = _context.Decks.First();

            // Hraci zacinaju s dvomi kartami
            await deck.GetCard(player.Name, _context);
            await deck.GetCard(player.Name, _context);
        }
    }
}
