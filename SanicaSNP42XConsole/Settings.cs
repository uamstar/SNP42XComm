using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanicaSNP42XConsole
{
    public sealed class Settings
    {
        public required ComportSettings Comport { get; set; } = null!;
    }
}
