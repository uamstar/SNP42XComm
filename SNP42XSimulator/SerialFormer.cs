using System;
using System.Collections.Generic;
using System.Text;

namespace SNP42XSimulator
{
    internal class SerialFormer
    {
        public delegate void MsgCompletedHandler(Span<byte> msg);
        public event MsgCompletedHandler MsgComplete;

        private byte[] _buf = new byte[512];
        private short _ptr = 0;
        private ushort _len = 0;

        private delegate void PushHandler(byte c);
        private PushHandler _doPush;

        internal SerialFormer()
        {
            _doPush = Push_Guard;
        }

        public void Push(byte c)
        {
            _doPush(c);
        }
        private void Push_Guard(byte c)
        {
            if(c == 0x23)
            {
                _len = 3;
                _buf[_ptr++] = c;
                _doPush = Push_Basic_telegram;
            }
            else if(c == 0x01)
            {
                _len = 5;
                _buf[_ptr++] = c;
                _doPush = Push_CMD_Head;
            }
        }
        private void Push_Basic_telegram(byte c)
        {
            _buf[_ptr++] = c;
            _len--;
            if (_len == 0)
            {
                Span<byte> msg = _buf.AsSpan(0, _ptr);
                DoMsgComplete(msg);
            }
        }
        private void Push_CMD_Head(byte c)
        {
            _buf[_ptr++] = c;
            _len--;
            if (_len == 0)
            {
                if(c == 0x53)
                {
                    _len = 5;
                    _doPush = Push_CMD_Remains;
                }
                else if(c == 0x43)
                {
                    _len = 7;
                    _doPush = Push_CMD_Remains;
                }
            }
        }
        private void Push_CMD_Remains(byte c)
        {
            _buf[_ptr++] = c;
            _len--;
            if (_len == 0)
            {
                Span<byte> msg = _buf.AsSpan(0, _ptr);
                DoMsgComplete(msg);
            }
        }
        private void DoMsgComplete(Span<byte> msg)
        {
            _ptr = 0;
            _doPush = Push_Guard;
            MsgComplete(msg);
        }
        // When an Exception is raised, the method should be call to reset the process.
        public void ResetBufOnException()
        {
            _ptr = 0;
            _doPush = Push_Guard;
        }
    }
}
