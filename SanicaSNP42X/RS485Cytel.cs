using System;
using System.Text;

namespace SanicaSNP42X
{
    public class RS485Cytel
    {
        private IProcLogger logger = new DefaultLogger();
        private byte[] addressBytes = new byte[2];
        private byte address;
        public byte Address { get { return this.address; } }

        public delegate void NoResponseHandler(RS485Cytel sender);
        public event NoResponseHandler NoResponse;
        private RS485Agent agent;
        private CmdRespTuple cmd_polling;
        private CmdRespTuple cmd_PS;
        private CmdRespTuple cmd_PC_Loop_On, cmd_PC_Loop_Off;
        private CmdRespTuple cmd_PC_Lock_Up, cmd_PC_Lock_Down;

        public delegate void PCResponseHandler(PC_Resp pcResp);
        public event PCResponseHandler ParkingControlResponse;

        private ParkingStatus parkingStatus = new ParkingStatus();
        public delegate void PSResponseHandler(ParkingStatus parkingStatus);
        public event PSResponseHandler ParkingPlateStatusResponse;

        /// <summary>
        /// initialize a message agent for a plate of address #(0 ~ 99)
        /// </summary>
        /// <param name="address">0 ~ 99</param>
        internal RS485Cytel(byte address, RS485Agent agent)
        {
            this.addressBytes[0] = (byte)(address / 10 + 0x30);
            this.addressBytes[1] = (byte)(address % 10 + 0x30);
            this.address = address;
            this.agent = agent;

            this.parkingStatus.address = address;

            byte[] cmd = new byte[4];
            cmd[0] = 0x23;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x05;
            cmd_polling = new CmdRespTuple(cmd, 0x01, 15);
            cmd_polling.Cytel = this;

            cmd = new byte[11];
            cmd[0] = 0x01;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x02;
            cmd[4] = 0x50;
            cmd[5] = 0x53;
            cmd[6] = 0x03;
            // 加上CRC
            byte[] crc = GetCRC(cmd, 4, 3);
            cmd[7] = crc[0];
            cmd[8] = crc[1];
            cmd[9] = crc[2];
            cmd[10] = crc[3];
            cmd_PS = new CmdRespTuple(cmd, 0x01, 35);
            cmd_PS.Cytel = this;

            cmd = new byte[13];
            cmd[0] = 0x01;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x02;
            cmd[4] = 0x50;
            cmd[5] = 0x43;
            cmd[6] = 0x32;      // Loop sensor control: on
            cmd[7] = 0x30;      // Locking control: keep status
            cmd[8] = 0x03;
            crc = GetCRC(cmd, 4, 5);    // 加上CRC
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            cmd[11] = crc[2];
            cmd[12] = crc[3];
            cmd_PC_Loop_On = new CmdRespTuple(cmd, 0x23, 4);
            cmd_PC_Loop_On.Cytel = this;

            cmd = new byte[13];
            cmd[0] = 0x01;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x02;
            cmd[4] = 0x50;
            cmd[5] = 0x43;
            cmd[6] = 0x31;      // Loop sensor control: off
            cmd[7] = 0x30;      // Locking control: keep current status
            cmd[8] = 0x03;
            crc = GetCRC(cmd, 4, 5);    // 加上CRC
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            cmd[11] = crc[2];
            cmd[12] = crc[3];
            cmd_PC_Loop_Off = new CmdRespTuple(cmd, 0x23, 4);
            cmd_PC_Loop_Off.Cytel = this;

            cmd = new byte[13];
            cmd[0] = 0x01;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x02;
            cmd[4] = 0x50;
            cmd[5] = 0x43;
            cmd[6] = 0x30;      // Loop sensor control: keep current status
            cmd[7] = 0x32;      // Locking control: on
            cmd[8] = 0x03;
            crc = GetCRC(cmd, 4, 5);    // 加上CRC
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            cmd[11] = crc[2];
            cmd[12] = crc[3];
            cmd_PC_Lock_Up = new CmdRespTuple(cmd, 0x23, 4);
            cmd_PC_Lock_Up.Cytel = this;

            cmd = new byte[13];
            cmd[0] = 0x01;
            cmd[1] = this.addressBytes[0];
            cmd[2] = this.addressBytes[1];
            cmd[3] = 0x02;
            cmd[4] = 0x50;
            cmd[5] = 0x43;
            cmd[6] = 0x30;      // Loop sensor control: keep current status
            cmd[7] = 0x31;      // Locking control: off
            cmd[8] = 0x03;
            crc = GetCRC(cmd, 4, 5);    // 加上CRC
            cmd[9] = crc[0];
            cmd[10] = crc[1];
            cmd[11] = crc[2];
            cmd[12] = crc[3];
            cmd_PC_Lock_Down = new CmdRespTuple(cmd, 0x23, 4);
            cmd_PC_Lock_Down.Cytel = this;
        }

        internal void SetLogger(IProcLogger logger) { this.logger = logger; }
        internal void NotifyNoResponse()
        {
            this.NoResponse(this);
        }
        private byte[] GetCRC(byte[] data, int start_index, int length)
        {
            byte[] tmp = new byte[length];
            Array.Copy(data, start_index, tmp, 0, length);
            int crc_num = CRC16ARC.CalcCRC(tmp);
            // 轉換成 ASCII 表示，4 bytes 的資料
            byte[] tmp2 = new byte[2];
            tmp2[1] = (byte)(crc_num & 0xFF);
            tmp2[0] = (byte)(crc_num >> 8);
            string hex_str = BitConverter.ToString(tmp2).Replace("-", "");

            logger.Debug($"hex_str: {hex_str}");

            byte[] bytes = Encoding.ASCII.GetBytes(hex_str);

            return bytes;
        }
        public void Polling() { this.agent.SendCmd(cmd_polling); }
        public void GetLockingPlateStatus() { this.agent.SendCmd(cmd_PS); }
        public void TurnLoopOn() { this.agent.SendCmd(cmd_PC_Loop_On); }
        public void TurnLoopOff() { this.agent.SendCmd(cmd_PC_Loop_Off); }
        public void TurnLockUp() { this.agent.SendCmd(cmd_PC_Lock_Up); }
        public void TurnLockDown() { this.agent.SendCmd(cmd_PC_Lock_Down); }
        internal void DataReceiving(byte[] data)
        {
            switch(data[3])
            {
                case 0x06:
                    ParkingControlResponse(PC_Resp.POSITIVE);
                    //logger.Info($"ParkingCtrlResp: {BitConverter.ToString(data)}");
                    break;
                case 0x15:
                    ParkingControlResponse(PC_Resp.NEGATIVE);
                    //logger.Info($"ParkingCtrlResp: {BitConverter.ToString(data)}");
                    break;
                case 0x02:
                    ParsePSResponse(data);
                    ParkingPlateStatusResponse(this.parkingStatus);
                    //logger.Info($"PSResponse: {BitConverter.ToString(data)}");
                    break;
                case 0x04:
                    logger.Info($"'End of transmission' is received: {BitConverter.ToString(data)}");
                    break;
                default:
                    logger.Warn($"Received unknown data: {BitConverter.ToString(data)}");
                    break;
            }
        }
        private void ParsePSResponse(byte[] data)
        {
            if (data.Length < 7) {
                logger.Warn($"received broken message. data length: {data.Length}");
                return; 
            }

            parkingStatus.loopSensorStatus = (LoopSensorStatus)data[6];
            parkingStatus.matSwitchStatus = (MatSwitchStatus)data[7];
            parkingStatus.lockPlateStatus = (LockPlateStatus)data[8];
            parkingStatus.sensorStatus = (SensorStatus)data[9];

            parkingStatus.loopCount = BytesToInt(data, 10);
            parkingStatus.offBaseCount = BytesToInt(data, 14);
            parkingStatus.onLevelCount = BytesToInt(data, 18);
            parkingStatus.offLevelCount = BytesToInt(data, 22);
            parkingStatus.onBaseCount = BytesToInt(data, 26);
        }
        private int BytesToInt(byte[] data, int start)
        {
            int result = -1;    // 超出 data 陣列範圍時，回傳值 -1
            if(start + 4 < data.Length) 
            {
                result = data[start++] & 0x0F * 1000
                        + data[start++] & 0x0F * 100
                        + data[start++] & 0x0F * 10
                        + data[start] & 0x0F;
            }               

            return result;
        }
    }
}
