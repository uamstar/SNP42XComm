using NLog;
using SNP42XSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSimulateSNP42X
{
    internal class ProcLogger : IProcLogger
    {
        protected static Logger? LOGGER = null;

        internal ProcLogger()
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
