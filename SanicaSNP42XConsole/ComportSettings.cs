using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanicaSNP42XConsole
{
    public sealed class ComportSettings
    {
        public required int BaudRate { get; set; } = 9600;
        public required int DataBits { get; set; } = 7;
        public required string Parity { get; set; } = "N";
        public required int StopBits { get; set; } = 1;
    }
}
