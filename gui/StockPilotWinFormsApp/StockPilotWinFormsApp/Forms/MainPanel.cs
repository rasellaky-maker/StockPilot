
using BO.Models;
using view;
using view.Mapps;
using System.Drawing;
using System.Windows.Forms;

namespace StockPilotWinFormsApp.Forms
{
    public partial class MainPanel : Form
    {
        private bool isClosing = false;
        private bool isLogOut = false;

        private WinFormView _view;

        private readonly System.Windows.Forms.Timer _refreshTimer =
            new() { Interval = 5000 };

        private bool _refreshing;

        public MainPanel(User user)
        {
            InitializeComponent();

            _view = new WinFormView(
                "https://localhost:7132/",
                labelFehler
            );

            _view.CoinSelected += OpenCoinChart;
            _view.localUser = user;

            // USER INFORMATION
            labelUserName.Text = user.Username;

            labelUserAvatar.Text = user.Username.Length > 0
                ? user.Username.Substring(0, 1).ToUpper()
                : "?";

            // FORM EVENTS
            FormClosing -= MainPanel_FormClosing;
            FormClosing += MainPanel_FormClosing;

            buttonLogout.Click -= buttonLogout_Click;
            buttonLogout.Click += buttonLogout_Click;

            // ==============================
            // LOGOUT ICON BACKGROUND FIX
            // ==============================

            // Place PictureBox inside Logout button
            buttonLogout.Controls.Add(pictureBoxLogout);

            pictureBoxLogout.Location = new Point(0, 0);
            pictureBoxLogout.Size = new Size(44, 46);
            pictureBoxLogout.BackColor = Color.Transparent;
            pictureBoxLogout.BringToFront();

            // Forward icon click to Logout button
            pictureBoxLogout.Click += buttonLogout_Click;

            // ==============================
            // NAVIGATION
            // ==============================

            buttonPortfolio.Click += (s, e) =>
                ShowTab(0);

            buttonMarket.Click += async (s, e) =>
            {
                ShowTab(1);
                await _view.LoadMarket(dataGridMarket);
            };

            ShowTab(0);

            // ==============================
            // INITIAL DATA
            // ==============================

            Shown += async (s, e) =>
            {
                await RefreshAllAsync(silent: false);

                await _view.LoadMarket(dataGridMarket);

                _refreshTimer.Start();
            };

            // ==============================
            // AUTO REFRESH
            // ==============================

            _refreshTimer.Tick += async (s, e) =>
            {
                await RefreshAllAsync(silent: true);
            };

            FormClosed += (s, e) =>
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };

            // ==============================
            // PORTFOLIO HOVER
            // ==============================

            buttonPortfolio.MouseEnter += (s, e) =>
            {
                pictureBoxPortfolio.BackColor =
                    buttonPortfolio.FlatAppearance.MouseOverBackColor;
            };

            buttonPortfolio.MouseLeave += (s, e) =>
            {
                pictureBoxPortfolio.BackColor =
                    buttonPortfolio.BackColor;
            };

            // ==============================
            // MARKET HOVER
            // ==============================

            buttonMarket.MouseEnter += (s, e) =>
            {
                pictureBoxMarket.BackColor =
                    buttonMarket.FlatAppearance.MouseOverBackColor;
            };

            buttonMarket.MouseLeave += (s, e) =>
            {
                pictureBoxMarket.BackColor =
                    buttonMarket.BackColor;
            };

            // ==============================
            // INITIAL ICON COLORS
            // ==============================

            pictureBoxPortfolio.BackColor =
                buttonPortfolio.BackColor;

            pictureBoxMarket.BackColor =
                buttonMarket.BackColor;

            // Logout icon is transparent.
            // It automatically inherits buttonLogout background.

            // ==============================
            // REFRESH MARKET
            // ==============================

            buttonRefresh.Click += async (s, e) =>
            {
                await _view.LoadMarket(dataGridMarket);
            };

            // ==============================
            // SEARCH MARKET
            // ==============================

            textBoxSearch.TextChanged += async (s, e) =>
            {
                await _view.SearchMarket(
                    dataGridMarket,
                    textBoxSearch.Text.Trim()
                );
            };
        }

        // ==============================
        // REFRESH PORTFOLIO
        // ==============================

        private async Task RefreshAllAsync(bool silent)
        {
            if (_refreshing) return;
            if (!Visible) return;
            if (WindowState == FormWindowState.Minimized) return;
            if (_view.localUser == null) return;

            _refreshing = true;

            try
            {
                await _view.LoadPortfolio(
                    flowPortfolio,
                    labelPortfolioEmpty,
                    labelUserHint,
                    silent
                );
            }
            catch
            {
            }
            finally
            {
                _refreshing = false;
            }
        }

        // ==============================
        // TAB NAVIGATION
        // ==============================

        private void ShowTab(int index)
        {
            tabController.SelectedIndex = index;

            labelPageTitle.Text = index == 0
                ? "Portfolio"
                : "Market";

            SetActive(buttonPortfolio, index == 0);
            SetActive(buttonMarket, index == 1);

            pictureBoxPortfolio.BackColor =
                buttonPortfolio.BackColor;

            pictureBoxMarket.BackColor =
                buttonMarket.BackColor;
        }

        // ==============================
        // BUTTON STYLE
        // ==============================

        private static void SetActive(Button b, bool active)
        {
            b.BackColor = active
                ? Color.FromArgb(23, 41, 26)
                : Color.FromArgb(14, 19, 27);

            b.ForeColor = active
                ? Color.FromArgb(182, 250, 100)
                : Color.FromArgb(140, 152, 169);

            b.FlatAppearance.MouseOverBackColor = active
                ? Color.FromArgb(23, 41, 26)
                : Color.FromArgb(20, 29, 35);
        }

        // ==============================
        // FORM CLOSING
        // ==============================

        private void MainPanel_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (isClosing || isLogOut)
                return;

            var result = MessageBox.Show(
                "Do you want to end the session?",
                "End Session Confirmation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                isClosing = true;
                Application.Exit();
            }
            else if (result == DialogResult.No)
            {
                e.Cancel = true;
                isLogOut = true;

                _refreshTimer.Stop();
                _view.localUser = null;

                FormLogIn login = new FormLogIn();
                login.Show();

                this.Hide();
            }
            else
            {
                e.Cancel = true;
            }
        }

        // ==============================
        // LOGOUT
        // ==============================

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            if (isLogOut)
                return;

            isLogOut = true;

            _refreshTimer.Stop();
            _view.localUser = null;

            FormLogIn login = new FormLogIn();
            login.Show();

            this.Close();
        }

        // ==============================
        // PORTFOLIO MOUSE ENTER
        // ==============================

        private void buttonPortfolio_MouseEnter(
            object sender,
            EventArgs e)
        {
        }

        // ==============================
        // OPEN COIN CHART
        // ==============================

        private async void OpenCoinChart(
            int coinId,
            string coinName)
        {
            try
            {
                await _view.OpenCoinChartAsync(
                    coinId,
                    coinName,
                    this
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Chart error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
