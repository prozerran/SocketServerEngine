using System;
using System.Reflection;
using log4net;

namespace SocketCommon
{
    /// <summary>
    /// Handles all logging related functions.
    /// </summary>
    public sealed class LogManager
    {
        private static readonly LogManager _instance = new LogManager();
        static LogManager() {}
        private LogManager() { }
        public static LogManager Instance { get { return _instance; } }

        public static void LogInfoLine(string message, params object[] args)
        {
            message += "\r\n";
            LogInfo(message, args);
        }

        public static void LogErrorLine(string message, params object[] args)
        {
            message += "\r\n";
            LogError(message, args);
        }

        public static void ConsoleWriteLine(string message, params object[] args)
        {
            message += "\r\n";
            ConsoleWrite(message, args);
        }

        public static void ConsoleWrite(string message, params object[] args)
        {
            string msg = ((args.Length == 0) ? message : string.Format(message, args));
            Console.Write(msg);
        }

        public static void LogInfo(string message, params object[] args)
        {
            try
            {
                string msg = ((args.Length == 0) ? message : string.Format(message, args));
                msg = msg.TrimEnd('\0');
#if DEBUG
                Console.Write(msg);
#else
                NetLogger.Instance.WriteInfoLog(msg);
#endif
            }
            finally
            {
                // error
            }
        }

        public static void LogError(string message, params object[] args)
        {
            try
            {
                string msg = ((args.Length == 0) ? message : string.Format(message, args));
                msg = msg.TrimEnd('\0');
#if DEBUG
                Console.Write(msg);
#else
                NetLogger.Instance.WriteErrorLog(msg);
#endif
            }
            finally
            {
                // error
            }
        }
    }
    /// <summary>
    /// Handles log when not in DEBUG.
    /// </summary>
    public sealed class NetLogger
    {
        private readonly ILog _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly NetLogger _instance = new NetLogger();
        static NetLogger() {}
        private NetLogger() {}
        public static NetLogger Instance { get { return _instance; } }

        public void WriteInfoLog(string message)
        {
            _log.Info(message);
        }

        public void WriteErrorLog(string message)
        {
            _log.Error(message);
        }

        public void WriteInfoLog(string message, params object[] args)
        {
            try
            {
                _log.Info((args.Length == 0) ? message : string.Format(message, args));
            }
            finally
            {
                // error
            }
        }

        public void WriteErrorLog(string message, params object[] args)
        {
            try
            {
                _log.Error((args.Length == 0) ? message : string.Format(message, args));
            }
            finally
            {
                // error
            }
        }
    }
}
