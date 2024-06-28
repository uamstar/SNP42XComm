using System;
using NLog;

namespace WebSimulateSNP42X
{
    internal class WorkLogger : IWorkLogger
    {
        protected static Logger? LOGGER = null;
        internal WorkLogger()
        {
            if (LOGGER == null)
                LOGGER = LogManager.GetCurrentClassLogger();
        }
        public void Debug(string msg)
        {
            LOGGER?.Debug(msg);
        }

        public void Error(string msg, Exception? ex = null)
        {
            LOGGER?.Error(ex, msg);
        }

        public void Info(string msg)
        {
            LOGGER?.Info(msg);
        }

        public void Warn(string msg)
        {
            LOGGER?.Warn(msg);
        }
    }
}
