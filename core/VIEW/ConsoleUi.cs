using BO.Models;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VIEW
{
    /// <summary>
    /// Everything that makes the console client look nice: colours (same palette as the website),
    /// banner, tables, panels, spinner, input helpers and an ASCII candlestick chart.
    /// </summary>
    public static class ConsoleUi
    {
        // ------------------------------------------------------------------
        // SETUP
        // ------------------------------------------------------------------
        public static bool UseColor { get; private set; } = true;

        [DllImport("kernel32.dll")] private static extern IntPtr GetStdHandle(int handle);
        [DllImport("kernel32.dll")] private static extern bool GetConsoleMode(IntPtr handle, out uint mode);
        [DllImport("kernel32.dll")] private static extern bool SetConsoleMode(IntPtr handle, uint mode);

        public static void Init()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch { }

            UseColor = !Console.IsOutputRedirected
                       && Environment.GetEnvironmentVariable("NO_COLOR") == null;

            if (UseColor && OperatingSystem.IsWindows())
            {
                try
                {
                    IntPtr handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                    if (!GetConsoleMode(handle, out uint mode) || !SetConsoleMode(handle, mode | 0x0004))
                        UseColor = false; // ENABLE_VIRTUAL_TERMINAL_PROCESSING not available
                }
                catch { UseColor = false; }
            }
        }

        // ------------------------------------------------------------------
        // COLOURS  (palette of the website: lime accent, red for "down")
        // ------------------------------------------------------------------
        private const string Esc = "\u001b[";

        private static string Rgb(int r, int g, int b, string s)
            => UseColor ? $"{Esc}38;2;{r};{g};{b}m{s}{Esc}0m" : s;

        public static string Accent(string s) => Rgb(182, 250, 100, s);
        public static string Danger(string s) => Rgb(240, 138, 126, s);
        public static string Muted(string s) => Rgb(140, 152, 169, s);
        public static string Dim(string s) => Rgb(104, 117, 133, s);
        public static string Text(string s) => Rgb(241, 245, 249, s);
        public static string Warning(string s) => Rgb(250, 204, 100, s);
        public static string Bold(string s) => UseColor ? $"{Esc}1m{s}{Esc}22m" : s;

        private static readonly Regex AnsiRegex = new("\u001b\\[[0-9;]*[A-Za-z]", RegexOptions.Compiled);

        public static int VisibleLength(string s) => AnsiRegex.Replace(s, "").Length;

        private static string PadRight(string s, int width)
            => s + new string(' ', Math.Max(0, width - VisibleLength(s)));

        private static string PadLeft(string s, int width)
            => new string(' ', Math.Max(0, width - VisibleLength(s))) + s;

        // ------------------------------------------------------------------
        // MESSAGES
        // ------------------------------------------------------------------
        private static void Block(string symbol, string message)
        {
            string[] lines = message.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) lines = new[] { "" };

            Console.WriteLine($" {symbol} {lines[0]}");
            for (int i = 1; i < lines.Length; i++)
                Console.WriteLine($"   {lines[i]}");
        }

        public static void Success(string message) => Block(Accent("✔"), message);
        public static void Error(string message) => Block(Danger("✘"), message);
        public static void Info(string message) => Block(Muted("●"), Muted(message));
        public static void Warn(string message) => Block(Warning("!"), message);

        // ------------------------------------------------------------------
        // BANNER, PROMPT, INPUT
        // ------------------------------------------------------------------
        public static void Banner(string server)
        {
            string[] logo =
            {
                "╔═╗┌┬┐┌─┐┌─┐┬┌─╔═╗┬┬  ┌─┐┌┬┐",
                "╚═╗ │ │ ││  ├┴┐╠═╝││  │ │ │ ",
                "╚═╝ ┴ └─┘└─┘┴ ┴╩  ┴┴─┘└─┘ ┴ "
            };

            Console.WriteLine();
            foreach (string line in logo)
                Console.WriteLine("  " + Text(line[..15]) + Accent(line[15..]));

            Console.WriteLine();
            Console.WriteLine("  " + Muted("Crypto market in your terminal") + Dim("  ·  " + server));
            Console.WriteLine("  " + Dim("Type ") + Accent("-?") + Dim(" to see all commands"));
            Console.WriteLine();
        }

        public static void Goodbye() => Console.WriteLine("\n  " + Muted("Goodbye!") + "\n");

        public static string? Prompt(string? username)
        {
            string who = string.IsNullOrEmpty(username) ? Dim("guest") : Accent(username);
            Console.WriteLine();
            Console.Write($"{who} {Accent("❯")} ");
            return Console.ReadLine();
        }

        public static string Ask(string label)
        {
            Console.Write($" {Muted(label + ":")} ");
            return (Console.ReadLine() ?? "").Trim();
        }

        public static string ReadSecret(string label = "Password")
        {
            Console.Write($" {Muted(label + ":")} ");

            var sb = new StringBuilder();
            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write("•");
                }
            }

            Console.WriteLine();
            return sb.ToString();
        }

        /// <summary>Returns true for "y" / "yes" / "j" / "ja".</summary>
        public static bool Confirm(string question)
        {
            Console.Write($" {Warning("?")} {question} {Dim("[y/N]")} ");
            string answer = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            return answer is "y" or "yes" or "j" or "ja";
        }

        /// <summary>Accepts "1,5" and "1.5". No thousand separators on purpose (so "1.5" is never read as 15).</summary>
        public static bool TryParseDecimal(string? text, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            string normalized = text.Trim().Replace(',', '.');
            return decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out value);
        }

        /// <summary>Asks until a positive number is entered. Empty line = cancel (returns null).</summary>
        public static decimal? AskDecimal(string label, string? preset = null)
        {
            string input = preset ?? Ask(label + Dim(" (empty = cancel)"));

            while (true)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;

                if (TryParseDecimal(input, out decimal value) && value > 0)
                    return value;

                Error("Please enter a number greater than zero.");
                input = Ask(label);
            }
        }

        /// <summary>Asks until a positive whole number is entered. Empty line = cancel (returns null).</summary>
        public static int? AskInt(string label, string? preset = null)
        {
            string input = preset ?? Ask(label + Dim(" (empty = cancel)"));

            while (true)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;

                if (int.TryParse(input, out int value) && value > 0)
                    return value;

                Error("Please enter a positive whole number.");
                input = Ask(label);
            }
        }

        // ------------------------------------------------------------------
        // NUMBER FORMATS
        // ------------------------------------------------------------------
        public static string Money(decimal value, CultureInfo culture)
        {
            decimal abs = Math.Abs(value);
            string format = abs >= 1 || abs == 0 ? "N2" : abs >= 0.01m ? "N4" : "N6";
            return value.ToString(format, culture) + " $";
        }

        public static string Quantity(decimal value, CultureInfo culture)
            => value.ToString("0.####", culture);

        public static string Change(decimal percent, CultureInfo culture)
        {
            if (percent == 0) return Muted("– 0" + culture.NumberFormat.NumberDecimalSeparator + "00 %");

            string text = $"{(percent > 0 ? "▲" : "▼")} {Math.Abs(percent).ToString("0.00", culture)} %";
            return percent > 0 ? Accent(text) : Danger(text);
        }

        // ------------------------------------------------------------------
        // TABLE / PANEL
        // ------------------------------------------------------------------
        public static void Table(string[] headers, List<string[]> rows, bool[]? rightAlign = null, string? title = null)
        {
            int columns = headers.Length;
            var widths = new int[columns];

            for (int i = 0; i < columns; i++)
                widths[i] = headers[i].Length;

            foreach (string[] row in rows)
                for (int i = 0; i < columns; i++)
                    widths[i] = Math.Max(widths[i], VisibleLength(row[i]));

            string Cell(string text, int i)
                => " " + (rightAlign != null && rightAlign[i] ? PadLeft(text, widths[i]) : PadRight(text, widths[i])) + " ";

            string Rule(char left, char middle, char right)
                => "  " + Dim(left + string.Join(middle.ToString(), widths.Select(w => new string('─', w + 2))) + right);

            string bar = Dim("│");

            if (title != null)
                Console.WriteLine("  " + Bold(Text(title)));

            Console.WriteLine(Rule('╭', '┬', '╮'));
            Console.WriteLine("  " + bar + string.Join(bar, headers.Select((h, i) => Cell(Muted(h.ToUpperInvariant()), i))) + bar);
            Console.WriteLine(Rule('├', '┼', '┤'));

            foreach (string[] row in rows)
                Console.WriteLine("  " + bar + string.Join(bar, row.Select((c, i) => Cell(c, i))) + bar);

            Console.WriteLine(Rule('╰', '┴', '╯'));
        }

        public static void Panel(string title, IEnumerable<(string Label, string Value)> rows)
        {
            var list = rows.ToList();
            int labelWidth = list.Count == 0 ? 0 : list.Max(r => VisibleLength(r.Label));

            var lines = list
                .Select(r => Muted(PadRight(r.Label, labelWidth)) + "  " + r.Value)
                .ToList();

            int inner = Math.Max(
                lines.Count == 0 ? 0 : lines.Max(VisibleLength),
                VisibleLength(title) + 4);
            inner = Math.Max(inner, 32);

            Console.WriteLine("  " + Dim("╭─ ") + Bold(Text(title)) + Dim(" " + new string('─', inner - VisibleLength(title) - 1) + "╮"));

            foreach (string line in lines)
                Console.WriteLine("  " + Dim("│") + " " + PadRight(line, inner) + " " + Dim("│"));

            Console.WriteLine("  " + Dim("╰" + new string('─', inner + 2) + "╯"));
        }

        // ------------------------------------------------------------------
        // SPINNER
        // ------------------------------------------------------------------
        public static async Task<T> Spin<T>(string label, Task<T> task)
        {
            if (!UseColor) return await task;

            string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            int i = 0;

            try { Console.CursorVisible = false; } catch { }

            try
            {
                while (!task.IsCompleted)
                {
                    Console.Write($"\r {Accent(frames[i++ % frames.Length])} {Muted(label)}{Esc}K");
                    await Task.WhenAny(task, Task.Delay(80));
                }
            }
            finally
            {
                Console.Write($"\r{Esc}K");
                try { Console.CursorVisible = true; } catch { }
            }

            return await task;
        }

        // ------------------------------------------------------------------
        // CANDLESTICK CHART (ASCII)
        // ------------------------------------------------------------------
        private sealed class Candle
        {
            public int Index;
            public decimal O, H, L, C;
        }

        private static List<Candle> BuildCandles(List<PricePoint> points, int count, DateTime t0, double spanMs)
        {
            double bucket = spanMs / count;
            var slots = new Candle?[count];

            foreach (PricePoint p in points)
            {
                int i = Math.Clamp((int)((p.Time - t0).TotalMilliseconds / bucket), 0, count - 1);
                Candle? c = slots[i];

                if (c == null)
                    slots[i] = new Candle { Index = i, O = p.Price, H = p.Price, L = p.Price, C = p.Price };
                else
                {
                    if (p.Price > c.H) c.H = p.Price;
                    if (p.Price < c.L) c.L = p.Price;
                    c.C = p.Price;
                }
            }

            var result = new List<Candle>();
            decimal? prevClose = null;

            for (int i = 0; i < count; i++)
            {
                Candle? c = slots[i];

                if (c != null)
                {
                    // like on a real exchange: a candle opens where the previous one closed
                    if (prevClose.HasValue)
                    {
                        c.O = prevClose.Value;
                        c.H = Math.Max(c.H, prevClose.Value);
                        c.L = Math.Min(c.L, prevClose.Value);
                    }
                }
                else if (prevClose.HasValue)
                {
                    c = new Candle { Index = i, O = prevClose.Value, H = prevClose.Value, L = prevClose.Value, C = prevClose.Value };
                }
                else continue;

                prevClose = c.C;
                result.Add(c);
            }

            return result;
        }

        private static string AxisPrice(decimal value, CultureInfo culture)
        {
            decimal abs = Math.Abs(value);
            return value.ToString(abs >= 100 ? "N0" : abs >= 1 ? "N2" : abs >= 0.01m ? "N4" : "N6", culture);
        }

        /// <summary>Draws candlesticks. Each candle uses two columns, so the plot is plotWidth wide.</summary>
        public static List<string> CandleChart(List<PricePoint> points, int plotWidth, int height, CultureInfo culture)
        {
            var lines = new List<string>();

            if (points.Count < 2)
            {
                lines.Add("  " + Muted("Not enough data for this range yet."));
                return lines;
            }

            int count = Math.Max(8, plotWidth / 2);
            int width = count * 2;

            DateTime t0 = points[0].Time;
            DateTime t1 = points[^1].Time;
            double spanMs = Math.Max((t1 - t0).TotalMilliseconds, 1);

            List<Candle> candles = BuildCandles(points, count, t0, spanMs);

            decimal min = candles.Min(c => c.L);
            decimal max = candles.Max(c => c.H);
            decimal pad = (max - min) * 0.04m;
            if (pad == 0) pad = Math.Abs(max) * 0.01m;
            if (pad == 0) pad = 1;
            decimal low = min - pad;
            decimal high = max + pad;

            int Row(decimal v) => Math.Clamp((int)Math.Round((double)((high - v) / (high - low)) * (height - 1)), 0, height - 1);

            // 0 = empty, 1 = up, 2 = down, 3 = grid
            var chars = new char[height, width];
            var colors = new int[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    chars[r, c] = ' ';

            int[] labelRows = { 0, (height - 1) / 2, height - 1 };

            foreach (int r in labelRows)
                for (int c = 0; c < width; c++)
                {
                    chars[r, c] = '·';
                    colors[r, c] = 3;
                }

            foreach (Candle candle in candles)
            {
                int col = candle.Index * 2;
                if (col >= width) continue;

                int color = candle.C >= candle.O ? 1 : 2;
                int rowHigh = Row(candle.H);
                int rowLow = Row(candle.L);
                int rowOpen = Row(candle.O);
                int rowClose = Row(candle.C);
                int bodyTop = Math.Min(rowOpen, rowClose);
                int bodyBottom = Math.Max(rowOpen, rowClose);

                for (int r = rowHigh; r <= rowLow; r++)
                {
                    chars[r, col] = '│';
                    colors[r, col] = color;
                }

                if (bodyTop == bodyBottom)
                {
                    chars[bodyTop, col] = '─';
                    colors[bodyTop, col] = color;
                }
                else
                {
                    for (int r = bodyTop; r <= bodyBottom; r++)
                    {
                        chars[r, col] = '█';
                        colors[r, col] = color;
                    }
                }
            }

            const int labelWidth = 11;

            for (int r = 0; r < height; r++)
            {
                string label = "";
                if (r == labelRows[0]) label = AxisPrice(high, culture);
                else if (r == labelRows[1]) label = AxisPrice((high + low) / 2, culture);
                else if (r == labelRows[2]) label = AxisPrice(low, culture);

                var sb = new StringBuilder();
                sb.Append("  ").Append(Dim(PadLeft(label, labelWidth))).Append(Dim(" ┤"));

                int c = 0;
                while (c < width)
                {
                    int color = colors[r, c];
                    var run = new StringBuilder();

                    while (c < width && colors[r, c] == color)
                    {
                        run.Append(chars[r, c]);
                        c++;
                    }

                    string text = run.ToString();
                    sb.Append(color switch
                    {
                        1 => Accent(text),
                        2 => Danger(text),
                        3 => Dim(text),
                        _ => text
                    });
                }

                lines.Add(sb.ToString());
            }

            // x axis
            lines.Add("  " + new string(' ', labelWidth) + Dim(" └" + new string('─', width)));

            string format = spanMs <= 36 * 3600 * 1000 ? "HH:mm" : "dd.MM";
            var axis = new char[width + 5];
            for (int i = 0; i < axis.Length; i++) axis[i] = ' ';

            for (int i = 0; i < 4; i++)
            {
                string text = t0.AddMilliseconds(spanMs * i / 3).ToString(format, culture);
                int pos = i == 3 ? width - text.Length : (width - text.Length) * i / 3;
                for (int k = 0; k < text.Length && pos + k < axis.Length; k++)
                    axis[pos + k] = text[k];
            }

            lines.Add("  " + new string(' ', labelWidth + 2) + Dim(new string(axis).TrimEnd()));

            return lines;
        }

        // ------------------------------------------------------------------
        // "DID YOU MEAN ...?"
        // ------------------------------------------------------------------
        public static string? ClosestMatch(string input, IEnumerable<string> candidates)
        {
            string? best = null;
            int bestDistance = 3; // up to two typos

            foreach (string candidate in candidates)
            {
                int d = Levenshtein(input, candidate);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = candidate;
                }
            }

            return best;
        }

        private static int Levenshtein(string a, string b)
        {
            var prev = new int[b.Length + 1];
            var curr = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++) prev[j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
                }
                (prev, curr) = (curr, prev);
            }

            return prev[b.Length];
        }
    }
}