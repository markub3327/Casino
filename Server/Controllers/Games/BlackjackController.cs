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
        // Referencia na Db
        private readonly Models.ApiContext _context;

        // Referencia na krupiera
        private Models.Croupier _croupier;

        // Zoznam akcii hraca v hre, ak je na rade
        public enum Actions
        {
            Hit,
            Stand,
            Double,
            Split,
            Exit
        }

        // Konstruktor hry
        public BlackjackController(Models.ApiContext context)
        {
            // Uloz spojenie s Db
            _context = context;

            // Vytvor menu akcii hracov
            CreateActionsMenu();

            // Pridaj krupiera ak neexistuje
            AddCroupier();

            // Hraci, ktori skoncili hru
            var standPlayers = _context.Players.Where(p =>
                (p.ActionId == "Stand" && p.Name != "Croupier-Blackjack" && p.GameId == "Blackjack"));

            // Ak existuju hraci
            if (standPlayers.Any())
            {
                // Vsetci hraci aktivny v hre
                var players = _context.Players.Where(p =>
                    (p.GameId == "Blackjack" && p.Name != "Croupier-Blackjack"))
                    .Include(p => p.Cards);

                // Ak vsetci hraci skoncili hru, hra krupier
                if (standPlayers.Count() == players.Count())
                {
                    // Hra krupiera
                    CroupierPlay();

                    // Vyhodnotenie hry
                    foreach (var p in players)
                    {
                        var res = Rules(p);

                        // Vyhra
                        if (res == 1)
                        {
                            Win(p);
                        }
                        // Prehra
                        else if (res == -1)
                        {
                            Lose(p);
                        }
                        // Remiza
                        else
                        {
                            Draw(p);
                        }
                    }

                    // Uvolni krupierove karty
                    FreeCards(_croupier);

                    // Uloz zmeny do Db
                    _context.SaveChanges();
                }
            }
        }

        // GET: casino/blackjack
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

        // GET: casino/actions
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

        // GET: casino/blackjack/players
        // GET: casino/blackjack/players?token=XXX
        [HttpGet("casino/{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> GetPlayers([FromQuery(Name = "token")]string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var playerDb = (await _context.Players.Where(p =>
                    (p.Token == token && p.GameId == "Blackjack"))
                    .Include(p => p.Cards).ToArrayAsync())[0];

                // Ak existuje hrac (s tokenom)
                if (playerDb != null)
                {
                    // Skontroluj karty
                    CheckCards(playerDb);

                    await _context.SaveChangesAsync();

                    return Ok(playerDb);
                }
                return NotFound();
            }

            // Vytvor pole hracov pri stole obsahujuce aj ich karty
            var players = await _context.Players.Where(p => (p.GameId == "Blackjack"))
                .Include(p => p.Cards).ToListAsync();

            // Vytvor vystup JSON zo zoznamu hracov pri stole
            return Ok(players.Select(p => new
            {
                name = p.Name,
                bet = p.Bet,
                cards = p.Cards.Select(c => new { suit = c.Suit, value = c.Value }),
                state = p.State
            }));
        }

        // POST: casino/players?token=XXX
        [HttpPost("casino/{controller}/players"), Produces("application/json")]
        public async Task<IActionResult> PostPlayer([FromBody]Items.Player player)
        {
            // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
            var playerDb = (await _context.Players.Where(p =>
                (p.Token == player.Token && p.GameId == null))
                .Include(p => p.Cards).ToArrayAsync())[0];

            // Ak existuje v Db hrac (s tokenom a nehra ziadnu hru)
            if (player != null)
            {
                // Nastav aktivnu hru hraca
                playerDb.GameId = "Blackjack";

                // Vyska stavky sa vynuluje
                playerDb.Bet = 0;

                // Nastav stav hraca na neaktivneho pri stole
                playerDb.State = Items.Player.EState.None;

                // Vynuluj akcie
                playerDb.ActionId = null;

                // Aktualizuj stav penazenky
                playerDb.Wallet = player.Wallet;

                // Uloz zmeny profilu hraca do Db
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlayers), new { token = playerDb.Token }, playerDb);
            }

            return BadRequest();
        }

        // PUT: casino/blackjack/player?token=XXX
        [HttpPut("casino/{controller}/players")]
        public async Task<IActionResult> PutPlayer([FromQuery(Name = "token")]string token, [FromBody]Items.Player player)
        {
            if (player != null)
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var playerDb = (await _context.Players.Where(p =>
                    (p.Token == token && p.GameId == "Blackjack"))
                    .Include(p => p.Cards).ToArrayAsync())[0];

                // Ak existuje v Db hrac (s tokenom)
                if (playerDb != null)
                {
                    if (playerDb.State != Items.Player.EState.Playing)
                    {
                        // Aktualizuje sa vyska stavky, ktoru ovplyvnuje hrac pocas hry
                        // Minimalna stavka je 100$
                        if (player.Bet < 100) playerDb.Bet = 100;
                        else playerDb.Bet = player.Bet;

                        // Nastav stav hraca na hrajuceho pri stole
                        playerDb.State = Items.Player.EState.Playing;

                        // Vezmi karty
                        await GivingCards(playerDb);
                    }
                    else
                    {
                        // Aktualizuj akciu hraca
                        playerDb.ActionId = player.ActionId;

                        // ... vykona akcie
                        switch (playerDb.ActionId)
                        {
                            case "Hit":
                                {
                                    var deck = _context.Decks.First();
                                    await deck.GetCard(playerDb.Name, _context);
                                    await _context.SaveChangesAsync();

                                    break;
                                }
                            case "Stand":
                                {
                                    Console.WriteLine($"Player {playerDb.Name} end the game.");

                                    // Neaktivny
                                    playerDb.State = Items.Player.EState.None;

                                    break;
                                }
                            case "Double":
                                {
                                    // Hrac dostane este kartu a hra konci
                                    var deck = _context.Decks.First();
                                    await deck.GetCard(playerDb.Name, _context);
                                    await _context.SaveChangesAsync();

                                    // Zdvojnasobi stavku
                                    playerDb.Bet <<= 1;

                                    // ... potom konci hru ...
                                    playerDb.ActionId = "Stand";
                                    // Neaktivny
                                    playerDb.State = Items.Player.EState.None;

                                    break;
                                }
                            case "Exit":
                                {
                                    _croupier.Wallet += playerDb.Bet >> 1;
                                    playerDb.Wallet -= playerDb.Bet >> 1;
                                    playerDb.State = Items.Player.EState.None;
                                    RoundEnd(playerDb);

                                    // Uvolni krupierove karty
                                    FreeCards(_croupier);

                                    break;
                                }
                        }
                    }

                    // Uloz zmeny profilu hraca do Db
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }

            return BadRequest();
        }

        // DELETE: casino/players?token=XXX
        [HttpDelete("casino/{controller}/players")]
        public async Task<IActionResult> DeletePlayer([FromQuery(Name = "token")]string token)
        {
            var player = (await _context.Players.Where(p =>
                (p.Token == token && p.GameId == "Blackjack"))
                .Include(p => p.Cards).ToArrayAsync())[0];

            if (player == null)
            {
                return NotFound();
            }

            RoundEnd(player);
            player.GameId = null;
            player.State = Items.Player.EState.None;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Uvolni hracove karty
        private void FreeCards(Items.Player player)
        {
            if (player.Cards.Any())
            {
                foreach (var c in player.Cards) // Uvolni karty
                {
                    c.PlayerId = string.Empty;
                }
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

        // Rozdavanie prvych kariet
        private async Task GivingCards(Items.Player player)
        {
            if (!player.Cards.Any())
            {
                // Novy hrac ak este nedostal karty
                var deck = _context.Decks.First();

                // Hraci zacinaju s dvomi kartami
                await deck.GetCard(player.Name, _context);
                await deck.GetCard(player.Name, _context);

                await _context.SaveChangesAsync();
            }

            CheckCards(player);
        }

        // Skontroluj sucet hracovych kariet
        private void CheckCards(Items.Player player)
        {
            var sum = player.CardSum;

            // Hra sa prerusila ak ziskal hrac viac alebo prave 21 sucet kariet
            if (sum >= 21)
            {
                // Hrac ukoncil hru a caka na vyhodnotenie
                player.ActionId = "Stand";

                // Neaktivny
                player.State = Items.Player.EState.None;
            }
        }

        // Ukoncenie kola
        private void RoundEnd(Items.Player player)
        {
            // Vynuluj hracov profil a karty
            player.Bet = 0;
            player.ActionId = null;
            FreeCards(player);
        }

        // Funkcia vyhry
        private void Win(Items.Player player)
        {
            _croupier.Wallet -= player.Bet;
            player.Wallet += player.Bet;
            player.State = Items.Player.EState.Win;
            RoundEnd(player);
        }

        // Funkcia prehry
        private void Lose(Items.Player player)
        {
            _croupier.Wallet += player.Bet;
            player.Wallet -= player.Bet;
            player.State = Items.Player.EState.Lose;
            RoundEnd(player);
        }

        // Funkcia remizy
        private void Draw(Items.Player player)
        {
            player.State = Items.Player.EState.Draw;
            RoundEnd(player);
        }

        // Krupierova hra
        private void CroupierPlay()
        {
            do
            {
                _croupier.Print();

                // Hra krupiera
                var Csum = _croupier.CardSum;
                if (Csum >= 17) break;

                // Potiahni kartu
                var deck = _context.Decks.First();
                deck.GetCard(_croupier.Name, _context).Wait();

                // Uloz do Db
                _context.SaveChanges();
            } while (true);
        }

        // Vytvvorenie prvkov a menu akcii hraca
        private void CreateActionsMenu()
        {
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
        }
    }
}
