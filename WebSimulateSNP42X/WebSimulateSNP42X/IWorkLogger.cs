using System;

namespace WebSimulateSNP42X
{
    public interface IWorkLogger
    {
        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg, Exception? ex = null);
    }
}
