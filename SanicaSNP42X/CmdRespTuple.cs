using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SanicaSNP42X
{
    internal class CmdRespTuple
    {
        internal SerialPort Comport;
        internal byte[] Cmd { get; }
        internal byte[] Resp;
        internal byte Head;
        internal byte[] buff = new byte[128];
        internal int RespSize { get { return Resp.Length; } }
        internal int Remains;    // 剩餘尚未讀取的資料長度
        internal int Ptr;        // 已讀取的資料長度
        internal bool IsTimeLimitExpired = false;
        internal delegate void PushHandler(byte b);
        internal PushHandler Push;
        internal RS485Cytel Cytel { get; set; }      // 發出這個 cmd 的 plate

        public CmdRespTuple(byte[] cmd, byte respHead, int respLength)
        {
            Cmd = cmd;
            Resp = new byte[respLength];
            Remains = respLength;
            Ptr = 0;
            Head = respHead;

            Push = Push_01_guard;
        }
        public void Recycle()
        {
            Remains = Resp.Length;
            Ptr = 0;
            Push = Push_01_guard;
        }
        /// <summary>
        /// 執行 讀取 comport 的動作，直到取得預定的資料長度
        /// </summary>
        public virtual void ReadComport()
        {
            while (Remains > 0 & !IsTimeLimitExpired)
            {
                if (Comport.BytesToRead > 0)
                {
                    try
                    {
                        int bytesToRead = Comport.Read(buff, 0, 128);
                        for(int i = 0; i < bytesToRead; i++)
                        {
                            Push(buff[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new SNP42XException(0x11, e);
                    }
                }

                Thread.Sleep(1);
            }

            IsTimeLimitExpired = false;
        }
        private void Push_01_guard(byte b)
        {
            if(b == Head)
            {
                Resp[Ptr++] = b;
                Remains--;
                Push = Push_02_collect_remains;
            }
        }
        private void Push_02_collect_remains(byte b)
        {
            if(Remains > 0)
            {
                Resp[Ptr++] = b;
                Remains--;
            }
        }
    }
}
