using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Timers;

namespace SanicaSNP42X
{
    /// <summary>
    /// This class is for sending commands and receiving responses.
    /// While a command is sent, it will wait the response until the length of data requested is collected 
    /// or waiting time limit has expired.
    /// </summary>
    internal class RS485Agent
    {
        private IProcLogger _logger;
        protected SerialPort _comport = new SerialPort();
        private short _openCount = 0;
        private Thread _workingThread;
        private Queue _cmdRespQueue;
        private CmdRespTuple _currentTuple;
        private const int CHECK_INTERVAL = 5000;   // 每 5 秒檢查一次
        private System.Timers.Timer _timer;
        
        public delegate void HandleDataReceived(byte[] data);
        public event HandleDataReceived OnDataReceived;

        public RS485Agent(String portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            _comport.BaudRate = baudRate;
            _comport.DataBits = dataBits;
            _comport.StopBits = stopBits;
            _comport.Parity = parity;
            _comport.PortName = portName;

            _logger = new DefaultLogger();

            _cmdRespQueue = Queue.Synchronized(new Queue());

            _timer = new System.Timers.Timer(CHECK_INTERVAL);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = false;
        }
        internal void SetLogger(IProcLogger logger) { _logger = logger; }
        public void Start()
        {
            while (true)
            {
                if (OpenComport())
                {
                    _workingThread = new Thread(DoReceiveDataThread);
                    _workingThread.Priority = ThreadPriority.Highest;
                    _workingThread.IsBackground = true;
                    _workingThread.Start();
                    Thread.Sleep(100);
                    break;
                }
                if (_openCount++ < 3)
                {
                    Thread.Sleep(1000);
                    _logger.Error("Fail to open COM Port.");
                    continue;
                }
                _logger.Error("Fail to open COM Port.");
                return;
            }
        }
        public void Close()
        {
            _comport.Close();
            _workingThread?.Abort();
        }
        public void SendCmd(CmdRespTuple cmd)
        {
            cmd.Comport = _comport;
            
            _cmdRespQueue.Enqueue(cmd);

            //SendData(_cmdRespTuple.Cmd);
        }
        private void SendData(byte[] data)
        {
            try
            {
                _comport.Write(data, 0, data.Length);
                _timer.Start();     // 開始計時，回覆訊息應該要在5秒內得到
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }
        private bool OpenComport()
        {
            bool flag = false;
            if (_comport.IsOpen)
            {
                return true;
            }

            try
            {
                _comport.Open();
                _comport.DiscardInBuffer();
                _comport.DiscardOutBuffer();
            }
            catch (UnauthorizedAccessException)
            {
                flag = true;
            }
            catch (IOException)
            {
                flag = true;
            }
            catch (ArgumentException)
            {
                flag = true;
            }
            if (flag)
            {
                return false;
            }
            _comport.DtrEnable = true;
            _comport.RtsEnable = true;
            return true;
        }
        private void DoReceiveDataThread()
        {
            while (_comport.IsOpen)
            {
                if(_cmdRespQueue.Count > 0)
                {
                    _currentTuple = (CmdRespTuple)_cmdRespQueue.Dequeue();
                    SendData(_currentTuple.Cmd);

                    _currentTuple.ReadComport();
                    _currentTuple.Recycle();

                    _timer.Stop();
                    OnDataReceived(_currentTuple.Resp);
                }
            }
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(_currentTuple != null)
            {   // 超過時間未得到回應
                _currentTuple.IsTimeLimitExpired = true;

                _timer.Stop();  // 停止計時

                // 傳出"未得到回應"訊息
                _currentTuple.Cytel.NotifyNoResponse();

            }
        }
    }
}
