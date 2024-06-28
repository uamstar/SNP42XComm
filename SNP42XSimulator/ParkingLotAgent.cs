using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Timers;

namespace SNP42XSimulator
{
    public class ParkingLotAgent
    {
        private static ParkingLotAgent _instance = new ParkingLotAgent();
        private IProcLogger _logger = new DefaultLogger();
        public IProcLogger Logger { set { _logger = value; } }

        // key: SNP42X address
        private Dictionary<int, SNP42XSimulator> _parkingSpaces = new Dictionary<int, SNP42XSimulator>();
        private Thread _workingThread;
        private SerialPort _comport = new SerialPort();
        private short _openCount = 0;
        private SerialFormer _serialFormer = new SerialFormer();

        public static ParkingLotAgent getInstance(string serialPortName)
        {
            _instance.SetSerialPort(serialPortName);

            return _instance;
        }
        private ParkingLotAgent()
        {
            _serialFormer.MsgComplete += HandleReceivedMsg;
        }
        private void SetSerialPort(string portName)
        {
            _comport.BaudRate = 9600;
            _comport.DataBits = 8;
            _comport.StopBits = StopBits.One;
            _comport.Parity = Parity.None;
            _comport.PortName = portName;
            _comport.ReadTimeout = 2000;
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
                if (_comport.BytesToRead > 0)
                {
                    int bytesToRead = _comport.BytesToRead;
                    try
                    {
                        for (int i = 0; i < bytesToRead; i++)
                        {
                            byte byteData = (byte)_comport.ReadByte();

                            _serialFormer.Push(byteData);

                        }
                    }
                    catch (IOException)
                    {
                        _serialFormer.ResetBufOnException();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _serialFormer.ResetBufOnException();
                    }
                    catch (TimeoutException)
                    {
                        _serialFormer.ResetBufOnException();
                    }
                    catch (Exception)
                    {
                        _serialFormer.ResetBufOnException();
                    }
                    
                }
                Thread.Sleep(1);
            }
            _logger.Info("Comport closed.");
        }
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
                throw new SNP42XSimulatorException(0x01);   // errCode 0x01: fail to open com port
            }
        }
        public void Stop()
        {
            _comport.Close();
            _workingThread?.Abort();
        }
        public void AddParkingSpace(ushort parkingSpaceAddr)
        {
            if (!_parkingSpaces.ContainsKey(parkingSpaceAddr))
            {
                SNP42XSimulator space = new SNP42XSimulator(parkingSpaceAddr);
                _parkingSpaces.Add(parkingSpaceAddr, space);
            }
        }
        public void SetLoopSensorStatus(ushort parkingSpaceAddr, LoopSensorStatus status)
        {
            if (_parkingSpaces.ContainsKey(parkingSpaceAddr))
            {
                SNP42XSimulator space = _parkingSpaces[parkingSpaceAddr];
                space.LoopSensorStatus = status;
            }
            else throw new SNP42XSimulatorException(0x04);  // errCode 0x04: parking space not exists.
        }
        public int ParkingSpacesSize()
        {
            return _parkingSpaces.Count;
        }
        private void SendData(byte[] data)
        {
            try
            {
                _comport.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }
        private void HandleReceivedMsg(Span<byte> msg)
        {
//            byte[] msgBytes = msg.ToArray();
//            _logger.Info($"received: {BitConverter.ToString(msgBytes)}");

            if (msg[0] == 0x23)
            {
                if(msg[3] == 0x05)
                {
                    int addr = (msg[1] & 0x0F) * 10 + (msg[2] & 0x0F);
                    SNP42XSimulator snp42x;
                    _parkingSpaces.TryGetValue(addr, out snp42x);

                    SendData(snp42x.GetPollingResponse());
                }
            }
            else if (msg[0] == 0x01)
            {
                int addr = (msg[1] & 0x0F) * 10 + (msg[2] & 0x0F);
                SNP42XSimulator snp42x;
                _parkingSpaces.TryGetValue(addr, out snp42x);

                if (msg[5] == 0x53)
                {
                    SendData(snp42x.GetPSResponse());
                }
                else if (msg[5] == 0x43)
                {
                    if (msg[6] == 0x31)
                    {
                        // set loop sensor control off
                        snp42x.ForceOffLoopSensor();
                    }
                    else if (msg[6] == 0x32)
                    {
                        // set loop sensor control on
                        snp42x.ForceOnLoopSensor();
                    }

                    if (msg[7] == 0x31)
                    {
                        // set locking control down
                        snp42x.LockingCtrlDown();
                    }
                    else if (msg[7] == 0x32)
                    {
                        // set locking control up
                        snp42x.LockingCtrlUp();
                    }

                    SendData(snp42x.GetRespPositive());
                }
            }
        }
    }
}
