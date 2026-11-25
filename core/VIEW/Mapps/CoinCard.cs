using System.Drawing;
using System.Windows.Forms;

namespace view.Mapps
{
    public class CoinCard : Panel
    {
        private static readonly Color Normal = Color.FromArgb(17, 23, 32);
        private static readonly Color HoverBack = Color.FromArgb(20, 29, 35);
        private static readonly Color BorderNormal = Color.FromArgb(32, 41, 54);
        private static readonly Color BorderHover = Color.FromArgb(59, 77, 50);
        private static readonly Color Up = Color.FromArgb(182, 250, 100);
        private static readonly Color Down = Color.FromArgb(240, 138, 126);

        private readonly Label _name = new();
        private readonly Label _details = new();
        private readonly Label _total = new();
        private readonly Label _hint = new();
        private decimal? _lastPrice;
        private bool _hover;

        public int CoinId { get; set; }
        public string CoinName { get; private set; } = "";

        /// <summary>Raised when the user clicks anywhere on the card.</summary>
        public event EventHandler? Clicked;

        public CoinCard()
        {
            Size = new Size(300, 176);
            BackColor = Normal;
            Margin = new Padding(0, 0, 16, 16);
            DoubleBuffered = true;

            _name.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _name.ForeColor = Color.FromArgb(241, 245, 249);
            _name.Location = new Point(20, 18);
            _name.AutoSize = true;

            _details.Font = new Font("Segoe UI", 10F);
            _details.ForeColor = Color.FromArgb(140, 152, 169);
            _details.Location = new Point(20, 52);
            _details.AutoSize = true;

            _total.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            _total.ForeColor = Up;
            _total.Location = new Point(20, 96);
            _total.AutoSize = true;

            _hint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _hint.ForeColor = Color.FromArgb(140, 152, 169);
            _hint.Text = "View chart  \u2192";
            _hint.Location = new Point(20, 142);
            _hint.AutoSize = true;

            Controls.AddRange(new Control[] { _name, _details, _total, _hint });

            // click + hover on the card and on every label inside it
            foreach (Control c in new Control[] { this, _name, _details, _total, _hint })
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => Clicked?.Invoke(this, EventArgs.Empty);
                c.MouseEnter += (s, e) => UpdateHover();
                c.MouseLeave += (s, e) => UpdateHover();
            }

            Paint += (s, e) =>
            {
                using var pen = new Pen(_hover ? BorderHover : BorderNormal);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };
        }

        private void UpdateHover()
        {
            bool inside = ClientRectangle.Contains(PointToClient(Cursor.Position));
            if (inside == _hover) return;

            _hover = inside;
            BackColor = inside ? HoverBack : Normal;
            _hint.ForeColor = inside ? Up : Color.FromArgb(140, 152, 169);
            Invalidate();
        }

        public void SetData(string name, decimal quantity, decimal price)
        {
            CoinName = name;
            _name.Text = name;
            _details.Text = $"{quantity:0.####} pcs  \u00b7  ${price:N2} each";
            _total.Text = $"${quantity * price:N2}";

            // price went up: green, went down: red, unchanged: colour stays
            if (_lastPrice.HasValue && price != _lastPrice.Value)
                _total.ForeColor = price > _lastPrice.Value ? Up : Down;

            _lastPrice = price;
        }
    }
}