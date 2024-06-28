using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanicaSNP42XDemo
{
    public sealed class ComportSettings
    {
        public required string PortName { get; set; } = "COM1";
        public required int BaudRate { get; set; } = 9600;
        public required int DataBits { get; set; } = 7;
        public required string Parity { get; set; } = "N";
        public required int StopBits { get; set; } = 1;
    }
}
