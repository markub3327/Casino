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
            // Vytvor pole hracov pri stole obsahujuce aj ich karty
            var players = await _context.Players.Where(p => (p.GameId == "Blackjack"))
                .Include(p => p.Cards).ToListAsync();

            if (players.Count() > 1)
            {
                var standPlayers = players.Where(p => (p.ActionId == "Stand"));

                Console.WriteLine("Hraci, ktori ukoncili hru = {0}", standPlayers.Count());

                // Ak existuju hraci
                if (standPlayers.Any())
                {
                    Console.WriteLine("Hraci v hre = {0}", players.Count());

                    // Ak vsetci hraci skoncili hru, hra krupier
                    if (standPlayers.Count() == (players.Count() - 1))
                    {
                        // Hra krupiera
                        CroupierPlay();

                        // Vyhodnotenie hry
                        foreach (var p in players.Where(p => p.Name != "Croupier-Blackjack"))
                        {
                            // Skontroluj stav hry podla pravidiel
                            Rules(p);

                            // Ukonci herne kolo hraca
                            RoundEnd(p);
                        }

                        // Vynuluj profil krupiera
                        _croupier.Bet = 0;
                        _croupier.ActionId = null;
                        _croupier.State = Items.Player.EState.None;

                        // Uloz zmeny do Db
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var playerDb = players.First(p => (p.Token == token));

                // Ak existuje hrac (s tokenom)
                if (playerDb != null)
                {
                    return Ok(playerDb);
                }
                return NotFound();
            }
           
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
                var players = _context.Players.Where(p => (p.GameId == "Blackjack"));

                // Nacitaj tabulku z Db, kde plati zhodnost tokenu!
                var playerDb = (await players.Where(p => (p.Token == token))
                    .Include(p => p.Cards).ToArrayAsync())[0];

                // Ak existuje v Db hrac (s tokenom)
                if (playerDb != null)
                {
                    // Ak hrac nehra hru
                    if (playerDb.State != Items.Player.EState.Playing)
                    {
                        // Ak je prvym hracom u stola priprav krupierove karty
                        if (!players.Where(p => (p.State == Items.Player.EState.Playing)).Any())
                        {
                            FreeCards(_croupier);
                            GivingCards(_croupier, 1).Wait();
                        }

                        // Aktualizuje sa vyska stavky, ktoru ovplyvnuje hrac pocas hry
                        // Minimalna stavka je 100$
                        if (player.Bet < 100) playerDb.Bet = 100;
                        else playerDb.Bet = player.Bet;

                        // Nastav stav hraca na hrajuceho pri stole
                        playerDb.State = Items.Player.EState.Playing;

                        // Vezmi karty
                        FreeCards(playerDb);
                        await GivingCards(playerDb, 2);
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
                                    await GivingCards(playerDb, 1);
                                    break;
                                }
                            case "Stand":
                                {
                                    Console.WriteLine($"Player {playerDb.Name} end the game.");
                                    break;
                                }
                            case "Double":
                                {
                                    // Hrac dostane este kartu a hra konci
                                    await GivingCards(playerDb, 1);

                                    // Zdvojnasobi stavku
                                    playerDb.Bet <<= 1;

                                    // ... potom konci hru ...
                                    playerDb.ActionId = "Stand";

                                    break;
                                }
                            case "Exit":
                                {
                                    if (playerDb.Cards.Count() == 2)
                                    {
                                        // Hrac ukoncil hru
                                        playerDb.State = Items.Player.EState.None;

                                        // Ukonci kolo
                                        RoundEnd(playerDb);

                                        // Strhni polovicu stavky
                                        _croupier.Wallet += playerDb.Bet >> 1;
                                        playerDb.Wallet -= playerDb.Bet >> 1;

                                        // Uvolni hracove karty
                                        FreeCards(playerDb);
                                    }

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

            // Vynuluj stav hraca
            player.State = Items.Player.EState.None;
            RoundEnd(player);
            FreeCards(player);
            player.GameId = null;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Uvolni hracove karty
        private void FreeCards(Items.Player player)
        {
            if (player.Cards.Any())
            {
                // Uvolni karty
                foreach (var c in player.Cards)
                {
                    c.PlayerId = string.Empty;
                }
            }
        }

        // Pravidla hry Blackjack
        private void Rules(Items.Player player)
        {
            var sum = player.CardSum;
            var Csum = _croupier.CardSum;

            Console.WriteLine($"sum = {sum}");
            Console.WriteLine($"Csum = {Csum}");

            // Hrac prehral - koniec hry ak prekrocil 21
            if (sum > 21)
            {
                player.State = Items.Player.EState.Lose;
            }
            // Hrac vyhral - koniec hry ak krupier prekrocil 21 a hrac nie
            // Ak ma hrac vyssi sucet kariet ako krupier
            else if ((Csum > 21) || (sum > Csum))
            {
                player.State = Items.Player.EState.Win;
            }
            // Ak ma rovnaky sucet ako krupier vrati sa mu stavka
            else if (sum == Csum)
            {
                player.State = Items.Player.EState.Draw;
            }
            // Inak prehra
            else
            {
                player.State = Items.Player.EState.Lose;
            }
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
                {
                    // Vytvor objekt balicka
                    var deck = new Models.Deck { CroupierId = _croupier.Name };

                    // Pridaj balicek do Db
                    _context.Decks.Add(deck);

                    // Vygeneruj karty
                    deck.Create(_context);
                    _context.SaveChanges();
                }

                // Krupier zacina s jednou kartou
                GivingCards(_croupier, 1).Wait();
            }
            else
            {
                // Obnov krupiera pre dalsie hry
                _croupier = croupier.First();
            }
        }

        // Rozdavanie prvych kariet
        private async Task GivingCards(Items.Player player, int count)
        {
            // Novy hrac ak este nedostal karty
            var deck = _context.Decks.First();

            // Hraci zacinaju s dvomi kartami
            for (int i = 0; i < count; i++)        
                await deck.GetCard(player.Name, _context);

            // Ulozi zmeny do Db
            await _context.SaveChangesAsync();

            // Hra sa prerusila ak ziskal hrac viac alebo prave 21 sucet kariet
            if (player.CardSum >= 21)
            {
                // Hrac ukoncil hru a caka na vyhodnotenie
                player.ActionId = "Stand";
            }
        }

        // Ukoncenie kola
        private void RoundEnd(Items.Player player)
        {
            // Vyhodnotenie kola podla stavu
            switch (player.State)
            {
                case Items.Player.EState.Win:
                    // Funkcia vyhry
                    {
                        _croupier.Wallet -= player.Bet;
                        player.Wallet += player.Bet;
                        break;
                    }
                case Items.Player.EState.Lose:
                    // Funkcia prehry
                    {
                        _croupier.Wallet += player.Bet;
                        player.Wallet -= player.Bet;
                        break;
                    }                
            }

            // Vynuluj hracov profil
            player.Bet = 0;
            player.ActionId = null;
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
                GivingCards(_croupier, 1).Wait();

                _croupier.ActionId = "Hit";

                // Uloz do Db
                _context.SaveChanges();
            } while (true);

            _croupier.ActionId = "Stand";
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
