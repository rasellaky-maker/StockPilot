using VIEW;

namespace ConsoleStockPilot
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var view = new ConsoleView("https://stockpilott.runasp.net/");

            // One-shot mode:  ConsoleStockPilot.exe -list   |   -get 3   |   -search btc   |   -track 3 24h
            if (args.Length > 0)
            {
                await view.ExecuteAsync(args);
                return 0;
            }

            // Interactive mode
            view.Welcome();

            while (true)
            {
                string? line = view.ReadCommand();
                if (line == null) break;                       // Ctrl+Z / Ctrl+D

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                if (!await view.ExecuteAsync(parts)) break;    // -exit
            }

            ConsoleUi.Goodbye();
            return 0;
        }
    }
}