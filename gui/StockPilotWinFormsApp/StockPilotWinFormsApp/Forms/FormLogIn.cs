using StockPilotWinFormsApp.Forms;
using System.Xml.Serialization;
using view;
using VIEW;

namespace StockPilotWinFormsApp
{
    public partial class FormLogIn : Form
    {
        private WinFormView _view = null;
        public FormLogIn()
        {
            this.Show();
            InitializeComponent();
            _view = new WinFormView("https://localhost:7132/", labelFehler);

            var green = Color.FromArgb(182, 250, 100);
            var gray = Color.FromArgb(32, 41, 54);

            textBoxUsername.Enter += (s, e) => panelUsernameBorder.BackColor = green;
            textBoxUsername.Leave += (s, e) => panelUsernameBorder.BackColor = gray;

            textBoxPassword.Enter += (s, e) => panelPasswordBorder.BackColor = green;
            textBoxPassword.Leave += (s, e) => panelPasswordBorder.BackColor = gray;
        }

        private void linkRegistrate_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            FormRegistrate formRegistrate = new FormRegistrate();
            formRegistrate.Show();
        }

        private async void buttonSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await _view.LogIn(textBoxUsername, textBoxPassword);
                if (result != null)
                {
                    MainPanel mainPanel = new MainPanel(result);
                    mainPanel.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void panelUsernameInner_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormLogIn_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
