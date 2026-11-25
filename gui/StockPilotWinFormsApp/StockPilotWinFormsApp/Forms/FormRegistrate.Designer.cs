namespace StockPilotWinFormsApp.Forms
{
    partial class FormRegistrate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureLogo = new PictureBox();
            labelBrand = new Label();
            labelTitle = new Label();
            labelSubtitle = new Label();
            panelCardBorder = new Panel();
            panel1 = new Panel();
            labelUsername = new Label();
            panelUsernameBorder = new Panel();
            panelUsernameInner = new Panel();
            textBoxUsername = new TextBox();
            labelPassword = new Label();
            panelPasswordBorder = new Panel();
            panelPasswordInner = new Panel();
            textBoxPassword = new TextBox();
            labelPasswordRepeat = new Label();
            panelPasswordRepeatBorder = new Panel();
            panelPasswordRepeatInner = new Panel();
            textBoxPasswordRepeat = new TextBox();
            labelFehler = new Label();
            buttonSubmit = new Button();
            linkLogin = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureLogo).BeginInit();
            panelCardBorder.SuspendLayout();
            panel1.SuspendLayout();
            panelUsernameBorder.SuspendLayout();
            panelUsernameInner.SuspendLayout();
            panelPasswordBorder.SuspendLayout();
            panelPasswordInner.SuspendLayout();
            panelPasswordRepeatBorder.SuspendLayout();
            panelPasswordRepeatInner.SuspendLayout();
            SuspendLayout();
            // 
            // pictureLogo
            // 
            pictureLogo.BackColor = Color.Transparent;
            pictureLogo.Image = Properties.Resources.stockpilot_icon;
            pictureLogo.Location = new Point(184, 28);
            pictureLogo.Name = "pictureLogo";
            pictureLogo.Size = new Size(72, 72);
            pictureLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureLogo.TabIndex = 0;
            pictureLogo.TabStop = false;
            // 
            // labelBrand
            // 
            labelBrand.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelBrand.ForeColor = Color.FromArgb(182, 250, 100);
            labelBrand.Location = new Point(0, 104);
            labelBrand.Name = "labelBrand";
            labelBrand.Size = new Size(440, 34);
            labelBrand.TabIndex = 1;
            labelBrand.Text = "StockPilot";
            labelBrand.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTitle
            // 
            labelTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(241, 245, 249);
            labelTitle.Location = new Point(0, 144);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(440, 42);
            labelTitle.TabIndex = 2;
            labelTitle.Text = "Create account";
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelSubtitle
            // 
            labelSubtitle.Font = new Font("Segoe UI", 10F);
            labelSubtitle.ForeColor = Color.FromArgb(140, 152, 169);
            labelSubtitle.Location = new Point(0, 188);
            labelSubtitle.Name = "labelSubtitle";
            labelSubtitle.Size = new Size(440, 24);
            labelSubtitle.TabIndex = 3;
            labelSubtitle.Text = "Join StockPilot and follow the market.";
            labelSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelCardBorder
            // 
            panelCardBorder.BackColor = Color.FromArgb(32, 41, 54);
            panelCardBorder.Controls.Add(panel1);
            panelCardBorder.Location = new Point(30, 232);
            panelCardBorder.Name = "panelCardBorder";
            panelCardBorder.Padding = new Padding(1);
            panelCardBorder.Size = new Size(380, 378);
            panelCardBorder.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(17, 23, 32);
            panel1.Controls.Add(labelUsername);
            panel1.Controls.Add(panelUsernameBorder);
            panel1.Controls.Add(labelPassword);
            panel1.Controls.Add(panelPasswordBorder);
            panel1.Controls.Add(labelPasswordRepeat);
            panel1.Controls.Add(panelPasswordRepeatBorder);
            panel1.Controls.Add(labelFehler);
            panel1.Controls.Add(buttonSubmit);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(1, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(378, 376);
            panel1.TabIndex = 0;
            // 
            // labelUsername
            // 
            labelUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelUsername.ForeColor = Color.FromArgb(140, 152, 169);
            labelUsername.Location = new Point(27, 24);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(324, 22);
            labelUsername.TabIndex = 0;
            labelUsername.Text = "USERNAME";
            labelUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelUsernameBorder
            // 
            panelUsernameBorder.BackColor = Color.FromArgb(32, 41, 54);
            panelUsernameBorder.Controls.Add(panelUsernameInner);
            panelUsernameBorder.Location = new Point(27, 48);
            panelUsernameBorder.Name = "panelUsernameBorder";
            panelUsernameBorder.Padding = new Padding(1);
            panelUsernameBorder.Size = new Size(324, 48);
            panelUsernameBorder.TabIndex = 1;
            // 
            // panelUsernameInner
            // 
            panelUsernameInner.BackColor = Color.FromArgb(12, 17, 24);
            panelUsernameInner.Controls.Add(textBoxUsername);
            panelUsernameInner.Dock = DockStyle.Fill;
            panelUsernameInner.Location = new Point(1, 1);
            panelUsernameInner.Name = "panelUsernameInner";
            panelUsernameInner.Size = new Size(322, 46);
            panelUsernameInner.TabIndex = 0;
            // 
            // textBoxUsername
            // 
            textBoxUsername.BackColor = Color.FromArgb(12, 17, 24);
            textBoxUsername.BorderStyle = BorderStyle.None;
            textBoxUsername.Font = new Font("Segoe UI", 12F);
            textBoxUsername.ForeColor = Color.FromArgb(241, 245, 249);
            textBoxUsername.Location = new Point(14, 12);
            textBoxUsername.Margin = new Padding(4);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(294, 22);
            textBoxUsername.TabIndex = 0;
            // 
            // labelPassword
            // 
            labelPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPassword.ForeColor = Color.FromArgb(140, 152, 169);
            labelPassword.Location = new Point(27, 106);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(324, 22);
            labelPassword.TabIndex = 2;
            labelPassword.Text = "PASSWORD";
            labelPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelPasswordBorder
            // 
            panelPasswordBorder.BackColor = Color.FromArgb(32, 41, 54);
            panelPasswordBorder.Controls.Add(panelPasswordInner);
            panelPasswordBorder.Location = new Point(27, 130);
            panelPasswordBorder.Name = "panelPasswordBorder";
            panelPasswordBorder.Padding = new Padding(1);
            panelPasswordBorder.Size = new Size(324, 48);
            panelPasswordBorder.TabIndex = 3;
            // 
            // panelPasswordInner
            // 
            panelPasswordInner.BackColor = Color.FromArgb(12, 17, 24);
            panelPasswordInner.Controls.Add(textBoxPassword);
            panelPasswordInner.Dock = DockStyle.Fill;
            panelPasswordInner.Location = new Point(1, 1);
            panelPasswordInner.Name = "panelPasswordInner";
            panelPasswordInner.Size = new Size(322, 46);
            panelPasswordInner.TabIndex = 0;
            // 
            // textBoxPassword
            // 
            textBoxPassword.BackColor = Color.FromArgb(12, 17, 24);
            textBoxPassword.BorderStyle = BorderStyle.None;
            textBoxPassword.Font = new Font("Segoe UI", 12F);
            textBoxPassword.ForeColor = Color.FromArgb(241, 245, 249);
            textBoxPassword.Location = new Point(14, 12);
            textBoxPassword.Margin = new Padding(4);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(294, 22);
            textBoxPassword.TabIndex = 0;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // labelPasswordRepeat
            // 
            labelPasswordRepeat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelPasswordRepeat.ForeColor = Color.FromArgb(140, 152, 169);
            labelPasswordRepeat.Location = new Point(27, 188);
            labelPasswordRepeat.Name = "labelPasswordRepeat";
            labelPasswordRepeat.Size = new Size(324, 22);
            labelPasswordRepeat.TabIndex = 4;
            labelPasswordRepeat.Text = "REPEAT PASSWORD";
            labelPasswordRepeat.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelPasswordRepeatBorder
            // 
            panelPasswordRepeatBorder.BackColor = Color.FromArgb(32, 41, 54);
            panelPasswordRepeatBorder.Controls.Add(panelPasswordRepeatInner);
            panelPasswordRepeatBorder.Location = new Point(27, 212);
            panelPasswordRepeatBorder.Name = "panelPasswordRepeatBorder";
            panelPasswordRepeatBorder.Padding = new Padding(1);
            panelPasswordRepeatBorder.Size = new Size(324, 48);
            panelPasswordRepeatBorder.TabIndex = 5;
            // 
            // panelPasswordRepeatInner
            // 
            panelPasswordRepeatInner.BackColor = Color.FromArgb(12, 17, 24);
            panelPasswordRepeatInner.Controls.Add(textBoxPasswordRepeat);
            panelPasswordRepeatInner.Dock = DockStyle.Fill;
            panelPasswordRepeatInner.Location = new Point(1, 1);
            panelPasswordRepeatInner.Name = "panelPasswordRepeatInner";
            panelPasswordRepeatInner.Size = new Size(322, 46);
            panelPasswordRepeatInner.TabIndex = 0;
            // 
            // textBoxPasswordRepeat
            // 
            textBoxPasswordRepeat.BackColor = Color.FromArgb(12, 17, 24);
            textBoxPasswordRepeat.BorderStyle = BorderStyle.None;
            textBoxPasswordRepeat.Font = new Font("Segoe UI", 12F);
            textBoxPasswordRepeat.ForeColor = Color.FromArgb(241, 245, 249);
            textBoxPasswordRepeat.Location = new Point(14, 12);
            textBoxPasswordRepeat.Margin = new Padding(4);
            textBoxPasswordRepeat.Name = "textBoxPasswordRepeat";
            textBoxPasswordRepeat.Size = new Size(294, 22);
            textBoxPasswordRepeat.TabIndex = 0;
            textBoxPasswordRepeat.UseSystemPasswordChar = true;
            // 
            // labelFehler
            // 
            labelFehler.Font = new Font("Segoe UI", 9.5F);
            labelFehler.ForeColor = Color.FromArgb(240, 138, 126);
            labelFehler.Location = new Point(27, 266);
            labelFehler.Name = "labelFehler";
            labelFehler.Size = new Size(324, 24);
            labelFehler.TabIndex = 6;
            labelFehler.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonSubmit
            // 
            buttonSubmit.BackColor = Color.FromArgb(182, 250, 100);
            buttonSubmit.Cursor = Cursors.Hand;
            buttonSubmit.FlatAppearance.BorderSize = 0;
            buttonSubmit.FlatAppearance.MouseDownBackColor = Color.FromArgb(127, 209, 58);
            buttonSubmit.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 255, 141);
            buttonSubmit.FlatStyle = FlatStyle.Flat;
            buttonSubmit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            buttonSubmit.ForeColor = Color.FromArgb(16, 21, 13);
            buttonSubmit.Location = new Point(27, 298);
            buttonSubmit.Margin = new Padding(4);
            buttonSubmit.Name = "buttonSubmit";
            buttonSubmit.Size = new Size(324, 48);
            buttonSubmit.TabIndex = 7;
            buttonSubmit.Text = "Create account";
            buttonSubmit.UseVisualStyleBackColor = false;
            // 
            // linkLogin
            // 
            linkLogin.ActiveLinkColor = Color.FromArgb(200, 255, 141);
            linkLogin.Font = new Font("Segoe UI", 10F);
            linkLogin.ForeColor = Color.FromArgb(140, 152, 169);
            linkLogin.LinkArea = new LinkArea(26, 6);
            linkLogin.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLogin.LinkColor = Color.FromArgb(182, 250, 100);
            linkLogin.Location = new Point(0, 630);
            linkLogin.Margin = new Padding(4, 0, 4, 0);
            linkLogin.Name = "linkLogin";
            linkLogin.Size = new Size(440, 28);
            linkLogin.TabIndex = 5;
            linkLogin.TabStop = true;
            linkLogin.Text = "Already have an account?  Log in";
            linkLogin.TextAlign = ContentAlignment.MiddleCenter;
            linkLogin.UseCompatibleTextRendering = true;
            linkLogin.VisitedLinkColor = Color.FromArgb(182, 250, 100);
            linkLogin.LinkClicked += linkLogin_LinkClicked;
            // 
            // FormRegistrate
            // 
            AcceptButton = buttonSubmit;
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(8, 11, 16);
            ClientSize = new Size(440, 680);
            Controls.Add(pictureLogo);
            Controls.Add(labelBrand);
            Controls.Add(labelTitle);
            Controls.Add(labelSubtitle);
            Controls.Add(panelCardBorder);
            Controls.Add(linkLogin);
            Font = new Font("Segoe UI", 12F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "FormRegistrate";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "StockPilot — Create account";
            FormClosed += FormRegistrate_FormClosed;
            ((System.ComponentModel.ISupportInitialize)pictureLogo).EndInit();
            panelCardBorder.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panelUsernameBorder.ResumeLayout(false);
            panelUsernameInner.ResumeLayout(false);
            panelUsernameInner.PerformLayout();
            panelPasswordBorder.ResumeLayout(false);
            panelPasswordInner.ResumeLayout(false);
            panelPasswordInner.PerformLayout();
            panelPasswordRepeatBorder.ResumeLayout(false);
            panelPasswordRepeatInner.ResumeLayout(false);
            panelPasswordRepeatInner.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureLogo;
        private Label labelBrand;
        private Label labelTitle;
        private Label labelSubtitle;
        private Panel panelCardBorder;
        private Panel panel1;
        private Label labelUsername;
        private Panel panelUsernameBorder;
        private Panel panelUsernameInner;
        private TextBox textBoxUsername;
        private Label labelPassword;
        private Panel panelPasswordBorder;
        private Panel panelPasswordInner;
        private TextBox textBoxPassword;
        private Label labelPasswordRepeat;
        private Panel panelPasswordRepeatBorder;
        private Panel panelPasswordRepeatInner;
        private TextBox textBoxPasswordRepeat;
        private Label labelFehler;
        private Button buttonSubmit;
        private LinkLabel linkLogin;
    }
}