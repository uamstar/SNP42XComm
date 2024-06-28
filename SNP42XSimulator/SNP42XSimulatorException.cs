using System;
using System.Collections.Generic;
using System.Text;

namespace SNP42XSimulator
{
    public class SNP42XSimulatorException : Exception
    {
        private byte errCode;
        private Exception exp;
        public Exception ExceptionCaught { get { return exp; } }

        public SNP42XSimulatorException(byte errCode)
        {
            this.errCode = errCode;
        }
        public SNP42XSimulatorException(byte errCode, Exception exp)
        {
            this.errCode = errCode;
            this.exp = exp;
        }
        public byte ErrCode { get { return errCode; } }
    }
}
