using System;
using System.Collections.Generic;
using System.Text;

namespace SanicaSNP42X
{
    public class SNP42XException : Exception
    {
        private byte errCode;
        private Exception exp;
        public Exception ExceptionCaught { get { return exp; } }

        public SNP42XException(byte errCode)
        {
            this.errCode = errCode;
        }
        public SNP42XException(byte errCode, Exception exp)
        {
            this.errCode = errCode;
            this.exp = exp;
        }
        public byte ErrCode { get { return errCode; } }
    }
}
