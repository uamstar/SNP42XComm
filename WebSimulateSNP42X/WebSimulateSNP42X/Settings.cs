using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSimulateSNP42X
{
    public sealed class Settings
    {
        public static Settings INSTANCE = new Settings()
        {
            ComportName = "COM1"
        };
        public required string ComportName { get; set; } = "COM1";
    }
}
