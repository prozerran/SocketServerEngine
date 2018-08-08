
using System.Configuration;

namespace SocketEngine
{
    public static class SocketCfg
    {
        public static string LogConnectionString = null;

        static SocketCfg()
        {
            try { LogConnectionString = ConfigurationManager.ConnectionStrings["db_table"].ToString(); }
            catch { }
        }
    }
}
