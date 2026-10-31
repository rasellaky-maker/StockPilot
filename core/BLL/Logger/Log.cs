using System.Diagnostics;

namespace BLL.Logger
{
    public static class Log
    {
        public static string Path = string.Empty;
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static readonly TimeZoneInfo ViennaTz = GetViennaTz();

        private static TimeZoneInfo GetViennaTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna"); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); }
        }

        private static string Now() =>
            TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ViennaTz).ToString("yyyy-MM-dd HH:mm:ss");

        public static async Task WriteLine(string text, string level = "INFO")
        {
            await _lock.WaitAsync();
            try
            {
                var dir = System.IO.Path.GetDirectoryName(Path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                await File.AppendAllTextAsync(Path,
                    $"{Now()} [{level}] {text}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LogWriter failed: {ex}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public static async Task<string> ReadText()
        {
            await _lock.WaitAsync();
            try
            {
                return File.Exists(Path) ? await File.ReadAllTextAsync(Path) : string.Empty;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}