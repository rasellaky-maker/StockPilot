
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using BO.Models;
using view.Mapps;

namespace view
{
    public class WinFormView
    {
        public event Action<int, string>? CoinSelected;

        private HttpSepClient.SepClient _client = null;
        public User localUser = null;

        private Label _labelLogInError = new Label();

        private readonly Dictionary<int, CoinCard> _cards = new();

        public WinFormView(string baseUrl, Label labelLogInError)
        {
            _labelLogInError = labelLogInError;

            _client = new HttpSepClient.SepClient(baseUrl);

            _labelLogInError.Visible = false;
        }

        // ==============================
        // LOGIN
        // ==============================

        public async Task<User?> LogIn(
            TextBox textBoxUsername,
            TextBox textBoxPassword)
        {
            _labelLogInError.Visible = false;

            if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                _labelLogInError.Text = "'Username' must be entered";
                _labelLogInError.Visible = true;
                return null;
            }

            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                _labelLogInError.Text = "'Password' must be entered";
                _labelLogInError.Visible = true;
                return null;
            }

            var response = await _client.HttpLogn(
                new User(
                    0,
                    textBoxUsername.Text,
                    textBoxPassword.Text,
                    "",
                    "",
                    0
                )
            );

            if (response.Status == HttpStatusCode.OK &&
                response.User != null)
            {
                localUser = response.User;

                MessageBox.Show(
                    $"Login successful!\nWelcome, {localUser.Username}!",
                    (int)response.Status + " " + response.Status,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return localUser;
            }

            MessageBox.Show(
                response.Error + "\nLogin failed. Please try again.",
                (int)response.Status + " " + response.Status,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            _labelLogInError.Visible = true;
            _labelLogInError.Text = response.Error;

            return null;
        }

        // ==============================
        // COIN CHART
        // ==============================

        public async Task OpenCoinChartAsync(
            int coinId,
            string coinName,
            IWin32Window owner)
        {
            var (status, points, error) =
                await _client.HttpGetHistory(coinId, "1h");

            if (status != HttpStatusCode.OK || points == null)
            {
                MessageBox.Show(
                    error ?? "Could not load coin history.",
                    "Chart error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            CoinChartForm chart = new CoinChartForm(coinName, points);

            chart.Show(owner);
        }

        // ==============================
        // PORTFOLIO
        // ==============================

        public async Task LoadPortfolio(
            FlowLayoutPanel flowPortfolio,
            Label labelPortfolioEmpty,
            Label labelUserHint,
            bool silent = false)
        {
            if (localUser == null)
            {
                labelPortfolioEmpty.Text = "No user is logged in.";
                labelPortfolioEmpty.Visible = true;
                return;
            }

            var (status, wallet, error) =
                await _client.HttpGetWallet(localUser.Id);

            if (status != HttpStatusCode.OK || wallet == null)
            {
                if (!silent)
                {
                    labelPortfolioEmpty.Text =
                        error ?? "Could not load your portfolio.";

                    labelPortfolioEmpty.Visible = true;
                }

                return;
            }

            labelUserHint.Text = $"Balance: ${wallet.Balance:N2}";

            flowPortfolio.SuspendLayout();

            try
            {
                var seen = new HashSet<int>();

                foreach (UsersCoins uc in wallet.UsersCoins)
                {
                    seen.Add(uc.Coin.Id);

                    if (_cards.TryGetValue(uc.Coin.Id, out CoinCard? card))
                    {
                        card.SetData(
                            uc.Coin.Name,
                            uc.Quantity,
                            uc.Coin.Value
                        );
                    }
                    else
                    {
                        card = new CoinCard();

                        card.CoinId = uc.Coin.Id;

                        card.SetData(
                            uc.Coin.Name,
                            uc.Quantity,
                            uc.Coin.Value
                        );

                        card.Clicked += (s, e) =>
                        {
                            if (s is CoinCard selectedCard)
                            {
                                CoinSelected?.Invoke(
                                    selectedCard.CoinId,
                                    selectedCard.CoinName
                                );
                            }
                        };

                        _cards[uc.Coin.Id] = card;

                        flowPortfolio.Controls.Add(card);
                    }
                }

                foreach (int id in _cards.Keys
                    .Where(k => !seen.Contains(k))
                    .ToList())
                {
                    flowPortfolio.Controls.Remove(_cards[id]);

                    _cards[id].Dispose();

                    _cards.Remove(id);
                }

                labelPortfolioEmpty.Text =
                    "You don't own any coins yet. Open the market to buy your first one.";

                labelPortfolioEmpty.Visible = _cards.Count == 0;
            }
            finally
            {
                flowPortfolio.ResumeLayout();
            }
        }

        // ==============================
        // MARKET GRID INITIALIZATION
        // ==============================

        private void InitializeMarketGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0)
            {
                grid.Columns.Add("Id", "ID");
                grid.Columns.Add("Name", "Coin");
                grid.Columns.Add("Price", "Price (EUR)");
                grid.Columns.Add("Buyers", "Buyers");
            }
        }

        // ==============================
        // LOAD MARKET
        // ==============================

        public async Task LoadMarket(DataGridView grid)
        {
            var (status, coins, error) = await _client.HttpGetAll();

            if (status != HttpStatusCode.OK || coins == null)
            {
                MessageBox.Show(
                    error ?? "Не удалось загрузить монеты."
                );

                return;
            }

            // Создаём столбцы перед добавлением строк
            InitializeMarketGrid(grid);

            grid.Rows.Clear();

            foreach (var coin in coins)
            {
                grid.Rows.Add(
                    coin.Id,
                    coin.Name,
                    coin.Value.ToString("N2"),
                    coin.NumberOfBuyers
                );
            }
        }

        // ==============================
        // SEARCH MARKET
        // ==============================

        public async Task SearchMarket(
            DataGridView grid,
            string search)
        {
            var (status, coins, error) = await _client.HttpGetAll();

            if (status != HttpStatusCode.OK || coins == null)
                return;

            // Проверяем наличие столбцов
            InitializeMarketGrid(grid);

            grid.Rows.Clear();

            var filteredCoins = coins.Where(c =>
                c.Name.Contains(
                    search ?? "",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            foreach (var coin in filteredCoins)
            {
                grid.Rows.Add(
                    coin.Id,
                    coin.Name,
                    coin.Value.ToString("N2"),
                    coin.NumberOfBuyers
                );
            }
        }
    }
}
