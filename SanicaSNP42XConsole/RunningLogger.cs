using System;
using SanicaSNP42X;
using NLog;

namespace SanicaSNP42XConsole
{
    internal class RunningLogger : IProcLogger
    {
        protected static Logger LOGGER = null;
        internal RunningLogger()
        {
            if (LOGGER == null)
                LOGGER = LogManager.GetCurrentClassLogger();
        }
        public void Debug(string msg)
        {
            LOGGER.Debug(msg);
        }

        public void Error(string msg, Exception ex = null)
        {
            LOGGER.Error(ex, msg);
            if(ex != null) LOGGER.Error(ex.Message);
        }

        public void Info(string msg)
        {
            LOGGER.Info(msg);
        }

        public void Warn(string msg)
        {
            LOGGER.Warn(msg);
        }
    }
}
