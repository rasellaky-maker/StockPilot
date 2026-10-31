using BLL.Configuration;

namespace SEP_WebApplication.Attributes
{
    internal static class MainConnnection
    {
        internal static string Connection = ConnectionStringProvider.GetConnectionString("GlobaleConnection");
    }
}
