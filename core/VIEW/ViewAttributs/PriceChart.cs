using System.Text;
using BO.Models;

namespace VIEW.ViewAttributs
{
    /// <summary>
    /// Zeichnet Kursdaten als Flächendiagramm aus Blockzeichen (▁▂▃▄▅▆▇█).
    /// Die Klasse kennt nur die Daten (PricePoint): kein HttpClient, keine Konsole beim Berechnen.
    ///
    ///   var chart = new PriceChart(points) { Title = "Bitrion · 24h", Height = 15 };
    ///   chart.Print();                  // farbig in die Konsole
    ///   string[] lines = chart.Render(); // oder nur als Text (Datei, TextBox, Test)
    ///
    /// Vor dem ersten Print() einmal setzen:  Console.OutputEncoding = Encoding.UTF8;
    /// </summary>
    public class PriceChart
    {
        private readonly List<PricePoint> _points;

        // ---------- Einstellungen ----------
        public string Title { get; set; } = "";

        /// <summary>Höhe des Diagramms in Zeilen.</summary>
        public int Height { get; set; } = 12;

        /// <summary>Breite des Diagramms in Spalten. 0 = an die Konsolenbreite anpassen.</summary>
        public int Width { get; set; } = 0;

        /// <summary>Grün bei steigendem, rot bei fallendem Kurs (nur bei Print()).</summary>
        public bool UseColor { get; set; } = true;

        // ---------- Kennzahlen aus den Daten ----------
        public int Count => _points.Count;
        public double Min => _points.Count == 0 ? 0 : (double)_points.Min(p => p.Price);
        public double Max => _points.Count == 0 ? 0 : (double)_points.Max(p => p.Price);
        public double First => _points.Count == 0 ? 0 : (double)_points[0].Price;
        public double Last => _points.Count == 0 ? 0 : (double)_points[^1].Price;

        /// <summary>Änderung vom ersten zum letzten Punkt als Bruch (0.05 = +5 %).</summary>
        public double Change => First == 0 ? 0 : (Last - First) / First;
        public bool IsUp => Last >= First;

        public PriceChart(IEnumerable<PricePoint> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            _points = points.OrderBy(p => p.Time).ToList();
        }

        /// <summary>Gibt das Diagramm farbig in der Konsole aus.</summary>
        public void Print()
        {
            var previous = Console.ForegroundColor;

            if (UseColor && _points.Count >= 2)
                Console.ForegroundColor = IsUp ? ConsoleColor.Green : ConsoleColor.Red;

            foreach (var line in Render())
                Console.WriteLine(line);

            Console.ForegroundColor = previous;
        }

        /// <summary>Baut das Diagramm als Textzeilen (ohne Farbe).</summary>
        public string[] Render()
        {
            if (_points.Count < 2)
                return new[] { "Слишком мало данных для графика." };

            const string blocks = " ▁▂▃▄▅▆▇█";
            const int labelWidth = 12;

            int height = Math.Max(3, Height);
            int width = Width > 0 ? Math.Max(20, Width) : Math.Max(20, GetConsoleWidth() - labelWidth - 3);

            var columns = Resample(width);

            // Wertebereich mit etwas Luft nach oben und unten.
            double min = columns.Min();
            double max = columns.Max();
            double pad = (max - min) * 0.1;
            if (pad == 0)
                pad = Math.Max(Math.Abs(max) * 0.01, 0.01);

            double lo = min - pad;
            double hi = max + pad;

            // Höhe jeder Spalte in Achtelblöcken (height Zeilen * 8 Stufen).
            int levels = height * 8;
            var heights = columns
                .Select(v => Math.Clamp((int)Math.Round((v - lo) / (hi - lo) * levels), 0, levels))
                .ToArray();

            var lines = new List<string>
        {
            $"{Title}   {Change:+0.00%;-0.00%}".Trim()
        };

            for (int row = height - 1; row >= 0; row--)
            {
                string label =
                    row == height - 1 ? hi.ToString("N2") :
                    row == height / 2 ? ((hi + lo) / 2).ToString("N2") :
                    row == 0 ? lo.ToString("N2") : "";

                var line = new StringBuilder();
                foreach (int h in heights)
                {
                    int cell = h - row * 8;
                    line.Append(cell >= 8 ? '█' : cell > 0 ? blocks[cell] : ' ');
                }

                lines.Add($"{label,labelWidth} │{line}");
            }

            lines.Add(new string(' ', labelWidth) + " └" + new string('─', width));

            string left = _points[0].Time.ToLocalTime().ToString("dd.MM HH:mm");
            string right = _points[^1].Time.ToLocalTime().ToString("dd.MM HH:mm");
            lines.Add(new string(' ', labelWidth + 2)
                      + left.PadRight(Math.Max(0, width - right.Length))
                      + right);

            return lines.ToArray();
        }

        // Punkte gibt es meist mehr als Spalten: pro Spalte den Durchschnitt bilden.
        private double[] Resample(int width)
        {
            var columns = new double[width];

            for (int i = 0; i < width; i++)
            {
                int start = i * _points.Count / width;
                int end = Math.Min(Math.Max(start + 1, (i + 1) * _points.Count / width), _points.Count);

                double sum = 0;
                for (int j = start; j < end; j++)
                    sum += (double)_points[j].Price;

                columns[i] = sum / (end - start);
            }

            return columns;
        }

        private static int GetConsoleWidth()
        {
            try
            {
                return Console.IsOutputRedirected ? 100 : Console.WindowWidth;
            }
            catch (IOException)
            {
                return 100; // keine Konsole vorhanden (z. B. WinForms-Anwendung)
            }
        }
    }
}