
using System;
using System.Threading.Tasks;
using VIEW;

namespace ConsoleStockPilot
{
    internal class Program
    {
        private static ConsoleView _view = new ConsoleView();

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Welcome to StockPilot");
                Console.WriteLine("Use -? to see optional arguments");

                string command;

                do
                {
                    Console.WriteLine();

                    command = Console.ReadLine() ?? "";

                    string[] arguments = command.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    Console.WriteLine();

                    if (arguments.Length == 0)
                        continue;

                    switch (arguments[0].ToLower())
                    {
                        case "-list":
                            Console.WriteLine(await _view.List());
                            break;

                        case "-get":
                            Console.WriteLine(await _view.Get());
                            break;

                        case "-search":
                            Console.WriteLine(await _view.Search());
                            break;

                        case "-registrate":
                            Console.WriteLine(await _view.SingIn());
                            break;

                        case "-login":
                            Console.WriteLine(await _view.LogIn());
                            break;

                        case "-mywallet":
                            Console.WriteLine(await _view.GetWallet());
                            break;

                        case "":
                            break;

                        case "-?":
                        case "?":
                            Console.WriteLine(_view.ShowMenu());
                            break;

                        case "-clear":
                            Console.Clear();
                            break;

                        case "-user":
                            Console.WriteLine(_view.UserInfo());
                            break;

                        case "-exit":
                            Console.WriteLine("Goodbye!");
                            break;

                        default:
                            Console.WriteLine("Unknown argument");
                            Console.WriteLine("Use -? to see optional arguments");
                            break;
                    }

                }
                while (!command.Trim().Equals(
                    "-exit",
                    StringComparison.OrdinalIgnoreCase
                ));
            }
            else
            {
                
            }
        }
    }
}