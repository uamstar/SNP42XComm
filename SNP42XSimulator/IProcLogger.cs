using System;
using System.Collections.Generic;
using System.Text;

namespace SNP42XSimulator
{
    public interface IProcLogger
    {
        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg, Exception ex = null);
    }
}
