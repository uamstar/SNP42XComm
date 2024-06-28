using System;
using System.Collections.Generic;
using System.Text;

namespace SanicaSNP42X
{
    internal class DefaultLogger : IProcLogger
    {
        public void Debug(string msg)
        {
            Console.WriteLine($"Debug: {msg}");
        }

        public void Error(string msg, Exception ex = null)
        {
            Console.Write($"ERROR: {msg}");
            if (ex != null) Console.WriteLine($", {ex.Message}");
        }

        public void Info(string msg)
        {
            Console.WriteLine($"Info: {msg}");
        }

        public void Warn(string msg)
        {
            Console.WriteLine($"Warn: {msg}");
        }
    }
}
