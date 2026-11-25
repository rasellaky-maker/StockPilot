using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using view;

namespace StockPilotWinFormsApp.Forms
{
    public partial class FormRegistrate : Form
    {
        private WinFormView _view = null;
        public FormRegistrate()
        {
            InitializeComponent();
            _view = new WinFormView("https://localhost:7132/", labelFehler);
            var green = Color.FromArgb(182, 250, 100);
            var gray = Color.FromArgb(32, 41, 54);

            textBoxUsername.Enter += (s, e) => panelUsernameBorder.BackColor = green;
            textBoxUsername.Leave += (s, e) => panelUsernameBorder.BackColor = gray;
            textBoxPassword.Enter += (s, e) => panelPasswordBorder.BackColor = green;
            textBoxPassword.Leave += (s, e) => panelPasswordBorder.BackColor = gray;
            textBoxPasswordRepeat.Enter += (s, e) => panelPasswordRepeatBorder.BackColor = green;
            textBoxPasswordRepeat.Leave += (s, e) => panelPasswordRepeatBorder.BackColor = gray;
        }

        private void FormRegistrate_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            FormLogIn login = new FormLogIn();
            login.Show();
        }
    }
}
