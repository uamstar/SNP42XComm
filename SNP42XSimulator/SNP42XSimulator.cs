using System;
using System.Threading;
using System.Threading.Tasks;

namespace SNP42XSimulator
{
    internal class SNP42XSimulator
    {
        private int _addr;
        private byte[] _addrBytes = { 0x30, 0x30 };
        private byte[] _Resp_POSITIVE = { 0x23, 0x00, 0x00, 0x06 };
        private byte[] _Resp_NEGATIVE = { 0x23, 0x00, 0x00, 0x15 };
        private byte[] _Resp_PS = { 0x01, 0x00, 0x00,   // address
                                    0x02, 0x50, 0x53,   // head
                                    0x31,               // Data structure 1st byte: Loop sensor status
                                    0x30,               // Data structure 2nd byte: Mat switch status
                                    0x30,               // Data structure 3rd byte: Lock plate status
                                    0x31,               // Data structure 4th byte: Sensor status
                                    0x30, 0x30, 0x30, 0x30, // 5th - 8th byte : Loop count
                                    0x30, 0x30, 0x30, 0x30, // 9th - 12th byte : Off base count
                                    0x30, 0x30, 0x30, 0x30, // 13th - 16th byte : On level count
                                    0x30, 0x30, 0x30, 0x30, // 17th - 20th byte : Off level count
                                    0x30, 0x30, 0x30, 0x30, // 21st - 24th byte : On base count
                                    0x03, 0x30, 0x30, 0x30, 0x30    // end & CRC
                                  };
        private const int LOOP_SENSOR_STATUS_INDEX = 6;
        private const int MAT_SWITCH_STATUS_INDEX = 7;
        private const int LOCK_PLATE_STATUS_INDEX = 8;
        private const int SENSOR_STATUS_INDEX = 9;
        private const int LOOP_COUNT_INDEX = 10;
        private const int OFF_BASE_COUNT_INDEX = 14;
        private const int ON_LEVEL_COUNT_INDEX = 18;
        private const int OFF_LEVEL_COUNT_INDEX = 22;
        private const int ON_BASE_COUNT_INDEX = 26;
        private const int CRC_INDEX = 31;

        private byte[] _Resp_Polling = {   0x01, 0x00, 0x00,   // address
                                        0x02, 0x50, 0x53,   // head
                                        0x31,               // Data structure 1st byte: Loop sensor status
                                        0x30,               // Data structure 2nd byte: Mat switch status
                                        0x30,               // Data structure 3rd byte: Lock plate status
                                        0x31,               // Data structure 4th byte: Sensor status
                                        0x03, 0x30, 0x30, 0x30, 0x30    // end & CRC
                                    };
        private const int CRC_FOR_LOOP_INDEX = 11;

        internal SNP42XSimulator(int addr)
        {
            _addr = addr;

            if (addr > 99 || addr < 0) throw new SNP42XSimulatorException(0x02);    // errCode 0x02: address is not in 0 ~ 99.

            _addrBytes[0] += (byte)(addr / 10);
            _addrBytes[1] += (byte)(addr % 10);

            _Resp_POSITIVE[1] = _addrBytes[0];
            _Resp_POSITIVE[2] = _addrBytes[1];
            _Resp_NEGATIVE[1] = _addrBytes[0];
            _Resp_NEGATIVE[2] = _addrBytes[1];
            _Resp_PS[1] = _addrBytes[0];
            _Resp_PS[2] = _addrBytes[1];
            _Resp_Polling[1] = _addrBytes[0];
            _Resp_Polling[2] = _addrBytes[1];
        }

        internal byte[] GetPSResponse()
        {
            // calculate CRC and set CRC bytes before returning values.
            ReadOnlySpan<byte> crcBytes = _Resp_PS.AsSpan(4, 26);
            int crcValue = CRC16ARC.CalcCRC(crcBytes);
            for (int i = 3; i >= 0; i--)
            {
                byte v = (byte)(crcValue & 0x0F);
                if (v < 10) _Resp_PS[CRC_INDEX + i] = (byte)(0x30 + v);
                else _Resp_PS[CRC_INDEX + i] = (byte)(0x37 + v);
                crcValue >>= 4;
            }
            return _Resp_PS;
        }
        internal byte[] GetPollingResponse()
        {   // calculate CRC and set CRC bytes before returning values.
            ReadOnlySpan<byte> crcBytes = _Resp_Polling.AsSpan(4, 7);
            int crcValue = CRC16ARC.CalcCRC(crcBytes);
            for (int i = 3; i >= 0; i--)
            {
                byte v = (byte)(crcValue & 0x0F);
                if (v < 10) _Resp_Polling[CRC_FOR_LOOP_INDEX + i] = (byte)(0x30 + v);
                else _Resp_Polling[CRC_FOR_LOOP_INDEX + i] = (byte)(0x37 + v);
                crcValue >>= 4;
            }
            return _Resp_Polling;
        }
        internal LoopSensorStatus LoopSensorStatus { get {return (LoopSensorStatus)_Resp_PS[LOOP_SENSOR_STATUS_INDEX]; } 
            set { 
                _Resp_PS[LOOP_SENSOR_STATUS_INDEX] = (byte)value;
                _Resp_Polling[LOOP_SENSOR_STATUS_INDEX] = (byte)value;
            } }
        internal MatSwitchStatus MatSwitchStatus { get { return (MatSwitchStatus)_Resp_PS[MAT_SWITCH_STATUS_INDEX]; } 
            set { 
                _Resp_PS[MAT_SWITCH_STATUS_INDEX] = (byte)value;
                _Resp_Polling[MAT_SWITCH_STATUS_INDEX] = (byte)value;
            } }
        internal LockPlateStatus LockPlateStatus { get { return (LockPlateStatus)_Resp_PS[LOCK_PLATE_STATUS_INDEX]; } 
            set { 
                _Resp_PS[LOCK_PLATE_STATUS_INDEX] = (byte)value;
                _Resp_Polling[LOCK_PLATE_STATUS_INDEX] = (byte)value;
            } }
        internal SensorStatus SensorStatus { get { return (SensorStatus)_Resp_PS[SENSOR_STATUS_INDEX]; } 
            set { 
                _Resp_PS[SENSOR_STATUS_INDEX] = (byte)value;
                _Resp_Polling[SENSOR_STATUS_INDEX] = (byte)value;
            } }
        internal byte[] GetRespPositive() { return _Resp_POSITIVE; }
        internal byte[] GetRespNegative() { return _Resp_NEGATIVE; }
        private void SetUshortAsByteStrAtIndexForRespPS(ushort c, int index)
        {
            for (int i = 3; i >= 0; i--)
            {
                byte v = (byte)(c & 0x0F);
                if (v < 10) _Resp_PS[index + i] = (byte)(0x30 + v);
                else _Resp_PS[index + i] = (byte)(0x37 + v);
                c >>= 4;
            }
        }
        private ushort _loopCount = 0;
        internal ushort LoopCount { get { return _loopCount; }
            set { 
                _loopCount = value;
                SetUshortAsByteStrAtIndexForRespPS(value, LOOP_COUNT_INDEX);
            }
        }
        private ushort _offBaseCount = 0;
        internal ushort OffBaseCount {  get { return _offBaseCount; }
            set {
                _offBaseCount = value;
                SetUshortAsByteStrAtIndexForRespPS(value, OFF_BASE_COUNT_INDEX);
            }
        }
        private ushort _onLevelCount = 0;
        internal ushort OnLevelCount {  get { return _onLevelCount; } 
            set {
                _onLevelCount= value;
                SetUshortAsByteStrAtIndexForRespPS(value, ON_LEVEL_COUNT_INDEX);
            } 
        }
        private ushort _offLevelCount = 0;
        internal ushort OffLevelCount { get { return _offLevelCount; }
            set {
                _offLevelCount = value;
                SetUshortAsByteStrAtIndexForRespPS(value, OFF_LEVEL_COUNT_INDEX);
            }
        }
        private ushort _onBaseCount = 0;
        internal ushort OnBaseCount {  get { return _onBaseCount; } 
            set {
                _onBaseCount = value;
                SetUshortAsByteStrAtIndexForRespPS(value, ON_BASE_COUNT_INDEX);
            } 
        }
        internal void ForceOnLoopSensor()
        {
            LoopSensorStatus = LoopSensorStatus.ForcedON;
        }
        internal void ForceOffLoopSensor()
        {
            LoopSensorStatus = LoopSensorStatus.ForcedOFF;
        }
        internal void LockingCtrlUp()
        {
            if(SensorStatus == SensorStatus.BottomEnd)
            {
                Task.Run(() =>
                {
                    SensorStatus = SensorStatus.Middle;
                    LockPlateStatus = LockPlateStatus.Inclining;
                    Thread.Sleep(5000);
                    SensorStatus = SensorStatus.TopEnd;
                    LockPlateStatus = LockPlateStatus.StandBy;
                });
            }
        }
        internal void LockingCtrlDown()
        {
            if (SensorStatus == SensorStatus.TopEnd)
            {
                Task.Run(() =>
                {
                    SensorStatus = SensorStatus.Middle;
                    LockPlateStatus = LockPlateStatus.Declining;
                    Thread.Sleep(5000);
                    SensorStatus = SensorStatus.BottomEnd;
                    LockPlateStatus = LockPlateStatus.StandBy;
                });
            }
        }
    }
}
