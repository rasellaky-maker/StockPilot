using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BO.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace view.Mapps
{
    public class CoinChartForm : Form
    {
        private readonly CartesianChart _chart = new();

        public CoinChartForm(string coinName, List<PricePoint> points)
        {
            Text = $"{coinName} - Price Chart";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(17, 23, 32);
            ForeColor = Color.White;

            var title = new Label
            {
                Text = $"{coinName} Price History",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(25, 20),
                AutoSize = true
            };

            var currentPrice = new Label
            {
                Text = points.Count > 0
                    ? $"${points[^1].Price:N2}"
                    : "No price data",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(182, 250, 100),
                Location = new Point(27, 65),
                AutoSize = true
            };

            _chart.Dock = DockStyle.Fill;
            _chart.BackColor = Color.FromArgb(17, 23, 32);

            _chart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = coinName,
                    Values = points.Select(p => (double)p.Price).ToArray(),
                    GeometrySize = 6,
                    Fill = null
                }
            };

            _chart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = points
                        .Select(p => p.Time.ToString("HH:mm"))
                        .ToArray(),
                    LabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(140, 152, 169))
                }
            };

            _chart.YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Price ($)"
                }
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = Color.FromArgb(17, 23, 32)
            };

            header.Controls.Add(title);
            header.Controls.Add(currentPrice);

            _chart.Dock = DockStyle.Fill;

            Controls.Add(_chart);
            Controls.Add(header);
        }
    }
}
