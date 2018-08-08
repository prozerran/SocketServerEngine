using System;
using System.Data;
using MySql.Data.MySqlClient;
using SocketCommon;

namespace SocketEngine
{
    /// <summary>
    /// Declare as singleton since we will have only ONE connection for all Socket threads.
    /// Handles connection and transactions to database.
    /// </summary>
    public sealed class SocketSQL : IDisposable
    {
        private static readonly SocketSQL _instance = new SocketSQL();
        public static SocketSQL Instance { get { return _instance; } }
        private MySqlConnection _dbConn = null;
        static SocketSQL() { }

        private SocketSQL()
        {
            _dbConn = new MySqlConnection(SocketCfg.LogConnectionString);
        }

        public void SetConnectionString(string connStr)
        {
            _dbConn = new MySqlConnection(connStr);
        }

        private bool Connect()
        {
            try
            {
                _dbConn.Open();
                return true;
            }
            catch
            {
                LogManager.LogErrorLine("Unable to connect to DB");
            }
            return false;
        }

        private bool Close()
        {
            try
            {
                _dbConn.Close();
                return true;
            }
            catch
            {
                LogManager.LogErrorLine("Unable to close DB");
            }
            return false;
        }

        public void Dispose()
        {
            if (_dbConn != null)
                Close();

            _dbConn.Dispose();
            _dbConn = null;
        }

        private bool IsConnected()
        {
            return (_dbConn != null && _dbConn.State == ConnectionState.Open);
        }

        public bool InsertToDatabase(string devModel, byte[] data, int len, out string error)
        {
            try
            {
                error = "";

                if (!IsConnected())
                {
                    Connect();
                }

                if (IsConnected())
                {
                    using (MySqlCommand dbCmd = _dbConn.CreateCommand())
                    {
                        dbCmd.CommandText = "SP_Socket_InsertToDatabase";
                        dbCmd.CommandType = CommandType.StoredProcedure;

                        dbCmd.Parameters.Add(new MySqlParameter("szDevModel", devModel));
                        dbCmd.Parameters.Add(new MySqlParameter("szDataGram", data)).Size = len;
                        dbCmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                error = String.Format("MySqlException InsertToDatabase: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                error = String.Format("Exception InsertToDatabase: {0}", ex.Message);
            }
            return false;
        }
    }
}
