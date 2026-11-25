using BO.Models;
using System.Globalization;
using System.Net;
using System.Text;
using view.HttpSepClient;
using Ui = VIEW.ConsoleUi;

namespace VIEW
{
    public class ConsoleView
    {
        private readonly SepClient sepClient;
        private readonly string _baseUrl;

        public User localUser = new User();
        public CultureInfo culture = new CultureInfo("de-DE");

        private static readonly string[] Ranges = { "1h", "24h", "7d", "30d" };

        private static readonly string[] KnownCommands =
        {
            "-list", "-get", "-search", "-track", "-chart",
            "-registrate", "-login", "-logout", "-user",
            "-mywallet", "-mycoin", "-createcoin", "-buy", "-sale",
            "-clear", "-?", "-exit"
        };

        public ConsoleView(string baseUrl)
        {
            _baseUrl = baseUrl;
            sepClient = new SepClient(baseUrl);
            Ui.Init();
        }

        private bool IsLoggedIn => localUser.Id > 0;

        // ==================================================================
        //  ENTRY POINTS (used by Program.cs)
        // ==================================================================

        public void Welcome() => Ui.Banner(_baseUrl);

        public string? ReadCommand() => Ui.Prompt(IsLoggedIn ? localUser.Username : null);

        /// <summary>Runs one command. Returns false when the user wants to leave.</summary>
        public async Task<bool> ExecuteAsync(string[] args)
        {
            if (args.Length == 0) return true;

            string command = args[0].ToLowerInvariant();
            if (!command.StartsWith('-') && command != "?")
                command = "-" + command;

            string[] rest = args.Skip(1).ToArray();

            try
            {
                switch (command)
                {
                    case "-list": await List(); break;
                    case "-get": await Get(rest); break;
                    case "-search": await Search(rest); break;
                    case "-track": await Track(rest); break;
                    case "-chart": await Chart(rest); break;

                    case "-registrate":
                    case "-register": await Register(); break;
                    case "-login": await LogIn(); break;
                    case "-logout": LogOut(); break;
                    case "-user": UserInfo(); break;

                    case "-mywallet":
                    case "-wallet": await ShowWallet(); break;
                    case "-mycoin": await MyCoins(); break;
                    case "-createcoin": await CreateCoin(); break;
                    case "-buy": await Buy(rest); break;
                    case "-sale":
                    case "-sell": await Sale(rest); break;

                    case "-clear":
                        Console.Clear();
                        Welcome();
                        break;

                    case "-?":
                    case "?":
                    case "-help": Help(); break;

                    case "-exit":
                    case "-quit": return false;

                    default:
                        Ui.Error($"Unknown command: {args[0]}");
                        string? suggestion = Ui.ClosestMatch(command, KnownCommands.Where(c => c.Length > 2));
                        if (suggestion != null)
                            Console.WriteLine("   " + Ui.Dim("Did you mean ") + Ui.Accent(suggestion) + Ui.Dim("?"));
                        else
                            Console.WriteLine("   " + Ui.Dim("Use ") + Ui.Accent("-?") + Ui.Dim(" to see all commands."));
                        break;
                }
            }
            catch (Exception ex)
            {
                Ui.Error("Unexpected error: " + ex.Message);
            }

            return true;
        }

        // ==================================================================
        //  HELPERS
        // ==================================================================

        private static void Fail(HttpStatusCode status, string? error, string fallback = "Something went wrong.")
        {
            Ui.Error(string.IsNullOrWhiteSpace(error) ? fallback : error.Trim().Trim('"'));
            Console.WriteLine("   " + Ui.Dim($"HTTP {(int)status} {status}"));
        }

        private bool RequireLogin()
        {
            if (IsLoggedIn) return true;

            Ui.Error("You must be logged in.");
            Console.WriteLine("   " + Ui.Dim("Use ") + Ui.Accent("-login") + Ui.Dim(" or ") + Ui.Accent("-registrate") + Ui.Dim("."));
            return false;
        }

        private static string? FirstLine(string? text)
            => text?.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        private static int SafeWindowWidth()
        {
            try { return Console.WindowWidth; }
            catch { return 100; }
        }

        private static int SafeWindowHeight()
        {
            try { return Console.WindowHeight; }
            catch { return 30; }
        }

        private string Money(decimal value) => Ui.Money(value, culture);

        private void PrintCoins(List<Coin> coins, string title)
        {
            var rows = coins
                .OrderBy(c => c.Id)
                .Select(c => new[]
                {
                    Ui.Dim(c.Id.ToString()),
                    Ui.Bold(Ui.Text(c.Name)),
                    Money(c.Value),
                    Ui.Muted(c.NumberOfBuyers.ToString())
                })
                .ToList();

            Ui.Table(new[] { "ID", "Coin", "Price", "Holders" }, rows, new[] { true, false, true, true }, title);
            Console.WriteLine("  " + Ui.Dim($"{coins.Count} coin(s)"));
        }

        private async Task ShowBalance()
        {
            var (status, wallet, _) = await sepClient.HttpGetWallet(localUser.Id);
            if (status == HttpStatusCode.OK && wallet != null)
                Console.WriteLine("   " + Ui.Muted("New balance: ") + Ui.Bold(Ui.Accent(Money(wallet.Balance))));
        }

        // ==================================================================
        //  HELP
        // ==================================================================

        public void Help()
        {
            void Section(string title, params (string Command, string Description, bool NeedsLogin)[] items)
            {
                Console.WriteLine();
                Console.WriteLine("  " + Ui.Bold(Ui.Text(title)));
                foreach (var (command, description, needsLogin) in items)
                {
                    string cmd = Ui.Accent(command);
                    int pad = Math.Max(2, 32 - Ui.VisibleLength(command));
                    Console.WriteLine("    " + cmd + new string(' ', pad) + Ui.Muted(description)
                                      + (needsLogin ? Ui.Dim("  · login") : ""));
                }
            }

            Section("Market",
                ("-list", "All coins with price and holders", false),
                ("-get [id]", "Details of one coin", false),
                ("-search [name]", "Find coins by name", false),
                ("-track <id> [1h|24h|7d|30d]", "Live candlestick chart (updates every 5 s)", false),
                ("-chart <id> [1h|24h|7d|30d]", "Candlestick chart, one shot", false));

            Section("Account",
                ("-registrate", "Create an account", false),
                ("-login", "Log in", false),
                ("-logout", "Log out", true),
                ("-user", "Show your account", false));

            Section("Trading",
                ("-mywallet", "Balance and your coins", true),
                ("-mycoin", "Coins you created", true),
                ("-createcoin", "Create your own coin", true),
                ("-buy [id] [qty]", "Buy a coin", true),
                ("-sale [id] [qty]", "Sell a coin", true));

            Section("Other",
                ("-clear", "Clear the screen", false),
                ("-?", "This help", false),
                ("-exit", "Leave StockPilot", false));

            Console.WriteLine();
            Console.WriteLine("  " + Ui.Dim("Tip: the dash is optional, 'list' works too. Commands also run directly: ConsoleStockPilot.exe -list"));
        }

        // ==================================================================
        //  MARKET
        // ==================================================================

        private async Task List()
        {
            var (status, coins, error) = await Ui.Spin("Loading market…", sepClient.HttpGetAll());

            if (status != HttpStatusCode.OK || coins == null)
            {
                Fail(status, error);
                return;
            }

            if (coins.Count == 0)
            {
                Ui.Info("There are no coins on the market yet.");
                return;
            }

            PrintCoins(coins, "Market");
        }

        private async Task Get(string[] a)
        {
            int? id = Ui.AskInt("Coin ID", a.Length > 0 ? a[0] : null);
            if (id == null) { Ui.Info("Cancelled."); return; }

            var (status, coin, error) = await Ui.Spin("Loading coin…", sepClient.HttpGetById(id.Value));

            if (status == HttpStatusCode.NotFound)
            {
                Ui.Error($"Coin with ID {id} was not found.");
                return;
            }

            if (status != HttpStatusCode.OK || coin == null)
            {
                Fail(status, error, $"Unable to retrieve coin {id}.");
                return;
            }

            Ui.Panel($"{coin.Name}  #{coin.Id}", new (string, string)[]
            {
                ("Price", Ui.Bold(Ui.Accent(Money(coin.Value)))),
                ("Holders", Ui.Text(coin.NumberOfBuyers.ToString())),
                ("Creator", Ui.Text(coin.UserId.HasValue ? $"user #{coin.UserId}" : "system")),
                ("Image", string.IsNullOrWhiteSpace(coin.Image) ? Ui.Dim("–") : Ui.Muted(coin.Image))
            });

            Console.WriteLine("  " + Ui.Dim("Tip: ") + Ui.Accent($"-track {coin.Id}") + Ui.Dim(" shows the live chart."));
        }

        private async Task Search(string[] a)
        {
            string term = a.Length > 0 ? string.Join(' ', a) : Ui.Ask("Search name");

            if (string.IsNullOrWhiteSpace(term))
            {
                Ui.Info("Please enter a search term.");
                return;
            }

            var (status, coins, error) = await Ui.Spin($"Searching for \"{term}\"…", sepClient.HttpSearch(term));

            if (status == HttpStatusCode.NotFound || (status == HttpStatusCode.OK && coins != null && coins.Count == 0))
            {
                Ui.Info("No results found. Try a different search.");
                return;
            }

            if (status != HttpStatusCode.OK || coins == null)
            {
                Fail(status, error);
                return;
            }

            PrintCoins(coins, $"Results for \"{term}\"");
        }

        // ==================================================================
        //  CHARTS
        // ==================================================================

        private bool TryReadChartArgs(string[] a, out int id, out string range)
        {
            id = 0;
            range = "1h";

            int? parsed = Ui.AskInt("Coin ID", a.Length > 0 ? a[0] : null);
            if (parsed == null) { Ui.Info("Cancelled."); return false; }
            id = parsed.Value;

            if (a.Length > 1)
            {
                string r = a[1].ToLowerInvariant();
                if (!Ranges.Contains(r))
                {
                    Ui.Error("Range must be one of: " + string.Join(", ", Ranges));
                    return false;
                }
                range = r;
            }

            return true;
        }

        private async Task<Coin?> LoadCoin(int id)
        {
            var (status, coin, error) = await Ui.Spin("Loading coin…", sepClient.HttpGetById(id));

            if (status == HttpStatusCode.NotFound)
            {
                Ui.Error($"Coin with ID {id} was not found.");
                return null;
            }

            if (status != HttpStatusCode.OK || coin == null)
            {
                Fail(status, error, $"Unable to retrieve coin {id}.");
                return null;
            }

            return coin;
        }

        private int ChartHeight() => Math.Clamp(SafeWindowHeight() - 14, 8, 18);

        private List<string> BuildChartFrame(Coin coin, string range, List<PricePoint>? points, int chartHeight, params string[] footer)
        {
            int plotWidth = Math.Clamp(SafeWindowWidth() - 18, 24, 110);
            var lines = new List<string>();

            lines.Add("  " + Ui.Bold(Ui.Text(coin.Name)) + Ui.Dim($"  #{coin.Id}"));

            if (points == null || points.Count < 2)
            {
                lines.Add("  " + Ui.Muted("Waiting for price data…"));
            }
            else
            {
                decimal first = points[0].Price;
                decimal last = points[^1].Price;
                decimal percent = first == 0 ? 0 : (last - first) / first * 100m;
                decimal high = points.Max(p => p.Price);
                decimal low = points.Min(p => p.Price);

                lines.Add("  " + Ui.Bold(Ui.Accent(Money(last))) + "   " + Ui.Change(percent, culture)
                          + Ui.Dim($"  over {range}")
                          + Ui.Dim("   high ") + Ui.Muted(Money(high))
                          + Ui.Dim("   low ") + Ui.Muted(Money(low)));
            }

            lines.Add("");
            lines.Add("  " + string.Join(" ", Ranges.Select(r => r == range ? Ui.Accent(Ui.Bold($"[{r}]")) : Ui.Dim($" {r} "))));
            lines.Add("");

            int chartStart = lines.Count;
            if (points != null && points.Count >= 2)
                lines.AddRange(Ui.CandleChart(points, plotWidth, chartHeight, culture));

            while (lines.Count < chartStart + chartHeight + 2)
                lines.Add("");

            lines.Add("");
            foreach (string line in footer)
                lines.Add("  " + line);

            return lines;
        }

        private async Task Chart(string[] a)
        {
            if (!TryReadChartArgs(a, out int id, out string range)) return;

            Coin? coin = await LoadCoin(id);
            if (coin == null) return;

            await ShowChartOnce(coin, range);
        }

        private async Task ShowChartOnce(Coin coin, string range)
        {
            var (status, points, error) = await Ui.Spin("Loading price history…", sepClient.HttpGetHistory(coin.Id, range));

            if (status != HttpStatusCode.OK || points == null)
            {
                Fail(status, error, "Price history is not available right now.");
                return;
            }

            Console.WriteLine();
            foreach (string line in BuildChartFrame(coin, range, points.OrderBy(p => p.Time).ToList(), ChartHeight(),
                         Ui.Dim("Tip: ") + Ui.Accent($"-track {coin.Id} {range}") + Ui.Dim(" for the live version.")))
                Console.WriteLine(line);
        }

        private static void DrawFrame(int top, int frameLines, List<string> lines)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < frameLines; i++)
                sb.Append(i < lines.Count ? lines[i] : "").Append("\u001b[K\n");

            Console.SetCursorPosition(0, top);
            Console.Write(sb.ToString());
        }

        private async Task Track(string[] a)
        {
            if (!TryReadChartArgs(a, out int id, out string range)) return;

            Coin? coin = await LoadCoin(id);
            if (coin == null) return;

            if (!Ui.UseColor || Console.IsInputRedirected)
            {
                await ShowChartOnce(coin, range);
                return;
            }

            const int refreshSeconds = 5;
            int chartHeight = ChartHeight();
            int frameLines = chartHeight + 10;

            Console.WriteLine();
            for (int i = 0; i < frameLines; i++) Console.WriteLine();
            int top = Math.Max(0, Console.CursorTop - frameLines);

            List<PricePoint>? points = null;
            string error = "";
            DateTime? updated = null;
            DateTime next = DateTime.MinValue;
            bool quit = false;

            try { Console.CursorVisible = false; } catch { }

            try
            {
                while (!quit)
                {
                    if (DateTime.UtcNow >= next)
                    {
                        var (status, fetched, err) = await sepClient.HttpGetHistory(id, range);

                        if (status == HttpStatusCode.OK && fetched != null)
                        {
                            points = fetched.OrderBy(p => p.Time).ToList();
                            updated = DateTime.Now;
                            error = "";
                        }
                        else
                        {
                            error = FirstLine(err) ?? $"HTTP {(int)status}";
                        }

                        next = DateTime.UtcNow.AddSeconds(refreshSeconds);
                    }

                    int secondsLeft = Math.Max(0, (int)Math.Ceiling((next - DateTime.UtcNow).TotalSeconds));

                    string status1 = Ui.Accent("●") + Ui.Muted(" live")
                                     + Ui.Dim(updated.HasValue ? $"  ·  updated {updated:HH:mm:ss}  ·  next in {secondsLeft}s" : "  ·  loading…");

                    if (error.Length > 0)
                        status1 += "  " + Ui.Danger("✘ " + error);

                    string keys = Ui.Dim("←/→ or 1-4: range   r: refresh   q: quit");

                    DrawFrame(top, frameLines, BuildChartFrame(coin, range, points, chartHeight, status1, keys));

                    // wait about a second, but react to key presses immediately
                    for (int i = 0; i < 10 && !quit; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKey key = Console.ReadKey(true).Key;
                            int index = Array.IndexOf(Ranges, range);
                            string newRange = range;

                            switch (key)
                            {
                                case ConsoleKey.Q:
                                case ConsoleKey.Escape:
                                    quit = true;
                                    break;
                                case ConsoleKey.R:
                                    next = DateTime.MinValue;
                                    break;
                                case ConsoleKey.LeftArrow:
                                    newRange = Ranges[(index + Ranges.Length - 1) % Ranges.Length];
                                    break;
                                case ConsoleKey.RightArrow:
                                    newRange = Ranges[(index + 1) % Ranges.Length];
                                    break;
                                case ConsoleKey.D1: case ConsoleKey.NumPad1: newRange = Ranges[0]; break;
                                case ConsoleKey.D2: case ConsoleKey.NumPad2: newRange = Ranges[1]; break;
                                case ConsoleKey.D3: case ConsoleKey.NumPad3: newRange = Ranges[2]; break;
                                case ConsoleKey.D4: case ConsoleKey.NumPad4: newRange = Ranges[3]; break;
                            }

                            if (newRange != range)
                            {
                                range = newRange;
                                points = null;
                                next = DateTime.MinValue;
                            }

                            break; // redraw right away
                        }

                        await Task.Delay(100);
                    }
                }
            }
            finally
            {
                try { Console.CursorVisible = true; } catch { }
            }

            Console.WriteLine();
        }

        // ==================================================================
        //  ACCOUNT
        // ==================================================================

        private async Task Register()
        {
            Console.WriteLine();
            Console.WriteLine("  " + Ui.Bold(Ui.Text("Create account")) + Ui.Dim("   (empty line = cancel)"));
            Console.WriteLine();

            string email;
            while (true)
            {
                email = Ui.Ask("E-Mail");
                if (email.Length == 0) { Ui.Info("Cancelled."); return; }
                if (email.Contains('@') && email.Contains('.')) break;
                Ui.Error("Please enter a valid email address.");
            }

            string username = Ui.Ask("Username");
            if (username.Length == 0) { Ui.Info("Cancelled."); return; }

            string password;
            while (true)
            {
                password = Ui.ReadSecret("Password");
                if (string.IsNullOrWhiteSpace(password))
                {
                    Ui.Error("Password cannot be empty.");
                    continue;
                }

                string repeat = Ui.ReadSecret("Repeat password");
                if (password == repeat) break;

                Ui.Error("Passwords do not match. Please try again.");
            }

            var user = new User(0, username, password, string.Empty, email, 0);

            var (status, singinUser, error) = await Ui.Spin("Creating your account…", sepClient.HttpSingIn(user));

            if (status == HttpStatusCode.Created && singinUser != null)
            {
                localUser = singinUser;
                Ui.Success($"Account created. Welcome aboard, {Ui.Accent(localUser.Username)}!");
                return;
            }

            Fail(status, error, "Registration failed. Please check your information and try again.");
        }

        private async Task LogIn()
        {
            Console.WriteLine();
            Console.WriteLine("  " + Ui.Bold(Ui.Text("Log in")) + Ui.Dim("   (empty line = cancel)"));
            Console.WriteLine();

            string username = Ui.Ask("Username");
            if (username.Length == 0) { Ui.Info("Cancelled."); return; }

            string password = Ui.ReadSecret("Password");
            if (string.IsNullOrWhiteSpace(password)) { Ui.Info("Cancelled."); return; }

            var user = new User(0, username, password, "", "", 0);

            var result = await Ui.Spin("Logging in…", sepClient.HttpLogn(user));

            if (result.Status == HttpStatusCode.OK && result.User != null)
            {
                localUser = result.User;
                Ui.Success($"Welcome back, {Ui.Accent(localUser.Username)}!");
                return;
            }

            Fail(result.Status, result.Error, "Login failed. Please try again.");
        }

        private void LogOut()
        {
            if (!IsLoggedIn)
            {
                Ui.Info("You are not logged in.");
                return;
            }

            string name = localUser.Username;
            localUser = new User();
            Ui.Success($"Logged out. See you soon, {name}!");
        }

        private void UserInfo()
        {
            if (!IsLoggedIn)
            {
                Ui.Info("You are not logged in. Use -login or -registrate.");
                return;
            }

            Ui.Panel("Your account", new (string, string)[]
            {
                ("ID", Ui.Text(localUser.Id.ToString())),
                ("Username", Ui.Bold(Ui.Accent(localUser.Username))),
                ("E-Mail", Ui.Text(string.IsNullOrWhiteSpace(localUser.Email) ? "–" : localUser.Email)),
                ("Free coin slots", Ui.Text(localUser.AvailableCoins.ToString()))
            });
        }

        // ==================================================================
        //  WALLET AND COINS
        // ==================================================================

        private async Task ShowWallet()
        {
            if (!RequireLogin()) return;

            var (status, wallet, error) = await Ui.Spin("Loading wallet…", sepClient.HttpGetWallet(localUser.Id));

            if (status != HttpStatusCode.OK || wallet == null)
            {
                Fail(status, error);
                return;
            }

            decimal portfolio = wallet.UsersCoins.Sum(uc => uc.Quantity * uc.Coin.Value);

            Ui.Panel($"{localUser.Username}'s wallet", new (string, string)[]
            {
                ("Balance", Ui.Bold(Ui.Accent(Money(wallet.Balance)))),
                ("Coins value", Ui.Text(Money(portfolio))),
                ("Total", Ui.Bold(Ui.Text(Money(wallet.Balance + portfolio))))
            });

            if (!wallet.UsersCoins.Any())
            {
                Console.WriteLine();
                Ui.Info("No coins in your wallet yet. Use -buy to get some.");
                return;
            }

            Console.WriteLine();

            var rows = wallet.UsersCoins
                .OrderByDescending(uc => uc.Quantity * uc.Coin.Value)
                .Select(uc =>
                {
                    decimal value = uc.Quantity * uc.Coin.Value;
                    decimal share = portfolio == 0 ? 0 : value / portfolio * 100m;

                    return new[]
                    {
                        Ui.Dim(uc.Coin.Id.ToString()),
                        Ui.Bold(Ui.Text(uc.Coin.Name)),
                        Ui.Quantity(uc.Quantity, culture),
                        Money(uc.Coin.Value),
                        Ui.Accent(Money(value)),
                        Ui.Muted(share.ToString("0.0", culture) + " %")
                    };
                })
                .ToList();

            Ui.Table(new[] { "ID", "Coin", "Qty", "Price", "Value", "Share" }, rows,
                new[] { true, false, true, true, true, true }, "Your coins");
        }

        private async Task MyCoins()
        {
            if (!RequireLogin()) return;

            var (status, coins, error) = await Ui.Spin("Loading your coins…", sepClient.HttpGetCreatedCoins(localUser.Id));

            if (status != HttpStatusCode.OK || coins == null)
            {
                Fail(status, error);
                return;
            }

            if (coins.Count == 0)
                Ui.Info("You haven't created any coins yet. Use -createcoin.");
            else
                PrintCoins(coins, "Coins you created");

            Console.WriteLine("  " + Ui.Muted("Free coin slots: ") + Ui.Bold(Ui.Accent(localUser.AvailableCoins.ToString())));
        }

        private async Task CreateCoin()
        {
            if (!RequireLogin()) return;

            if (localUser.AvailableCoins <= 0)
            {
                Ui.Warn("You have no free coin slots left, so you can't create another coin.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("  " + Ui.Bold(Ui.Text("Create a coin")) + Ui.Dim($"   ({localUser.AvailableCoins} free slot(s), empty line = cancel)"));
            Console.WriteLine();

            string name = Ui.Ask("Coin name");
            if (name.Length == 0) { Ui.Info("Cancelled."); return; }

            decimal? value = Ui.AskDecimal("Start price in $");
            if (value == null) { Ui.Info("Cancelled."); return; }

            string image = Ui.Ask("Image URL " + Ui.Dim("(optional)"));

            Console.WriteLine();
            Ui.Panel("New coin", new (string, string)[]
            {
                ("Name", Ui.Bold(Ui.Text(name))),
                ("Start price", Ui.Accent(Money(value.Value))),
                ("Image", string.IsNullOrWhiteSpace(image) ? Ui.Dim("–") : Ui.Muted(image))
            });

            if (!Ui.Confirm("Create this coin? It uses one of your free slots."))
            {
                Ui.Info("Cancelled.");
                return;
            }

            var coin = new Coin { Name = name, Value = value.Value, Image = image };
            var (status, created, error) = await Ui.Spin("Creating coin…", sepClient.HttpCreateCoin(localUser.Id, coin));

            if (status == HttpStatusCode.OK && created != null)
            {
                if (localUser.AvailableCoins > 0) localUser.AvailableCoins--;
                Ui.Success($"Coin {Ui.Accent(created.Name)} created with ID {created.Id}.");
                return;
            }

            Fail(status, error, "The coin could not be created.");
        }

        // ==================================================================
        //  TRADING
        // ==================================================================

        private async Task Buy(string[] a)
        {
            if (!RequireLogin()) return;

            int? id = Ui.AskInt("Coin ID", a.Length > 0 ? a[0] : null);
            if (id == null) { Ui.Info("Cancelled."); return; }

            decimal? quantity = Ui.AskDecimal("Quantity", a.Length > 1 ? a[1] : null);
            if (quantity == null) { Ui.Info("Cancelled."); return; }

            Coin? coin = await LoadCoin(id.Value);
            if (coin == null) return;

            decimal cost = coin.Value * quantity.Value;

            if (!Ui.Confirm($"Buy {Ui.Quantity(quantity.Value, culture)} × {coin.Name} for about {Money(cost)}?"))
            {
                Ui.Info("Cancelled.");
                return;
            }

            var (status, _, error) = await Ui.Spin("Buying…", sepClient.HttpBuy(id.Value, localUser.Id, quantity.Value));

            switch (status)
            {
                case HttpStatusCode.OK:
                    Ui.Success($"Purchase successful: {Ui.Quantity(quantity.Value, culture)} × {Ui.Accent(coin.Name)}.");
                    await ShowBalance();
                    break;

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    Ui.Error("Access denied. Check your API key or login.");
                    break;

                case HttpStatusCode.NotFound:
                    Ui.Error($"Coin with ID {id} was not found.");
                    break;

                case HttpStatusCode.BadRequest:
                    Ui.Error("Purchase failed: " + (error ?? "unknown reason").Trim().Trim('"'));
                    break;

                default:
                    Fail(status, error, "Unexpected error.");
                    break;
            }
        }

        private async Task Sale(string[] a)
        {
            if (!RequireLogin()) return;

            int? id = Ui.AskInt("Coin ID", a.Length > 0 ? a[0] : null);
            if (id == null) { Ui.Info("Cancelled."); return; }

            decimal? quantity = Ui.AskDecimal("Quantity", a.Length > 1 ? a[1] : null);
            if (quantity == null) { Ui.Info("Cancelled."); return; }

            Coin? coin = await LoadCoin(id.Value);
            if (coin == null) return;

            decimal estimate = coin.Value * quantity.Value;

            if (!Ui.Confirm($"Sell {Ui.Quantity(quantity.Value, culture)} × {coin.Name} for about {Money(estimate)}?"))
            {
                Ui.Info("Cancelled.");
                return;
            }

            var result = await Ui.Spin("Selling…", sepClient.HttpSaleCoin(localUser.Id, id.Value, quantity.Value));

            if (result.Status != HttpStatusCode.OK)
            {
                Fail(result.Status, result.Error, "The coin could not be sold.");
                return;
            }

            Ui.Success($"Coin sold. Revenue: {Ui.Bold(Ui.Accent(Money(result.SalePreis)))}");
            await ShowBalance();
        }
    }
}