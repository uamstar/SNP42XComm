using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SanicaSNP42X
{
    public class Manager
    {
        public const string VER = "0.1";
        private static Manager INSTANCE = new Manager();
        private RS485Agent rs485agent;
        private IProcLogger logger = new DefaultLogger();
        private Dictionary<int, RS485Cytel> cytelDict = new Dictionary<int, RS485Cytel>();

        public static Manager getInstance(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
        {
            INSTANCE.OpenRS485(portName, baudRate, parity, dataBits, stopBits);
            return INSTANCE; 
        }
        private Manager() { }
        private void OpenRS485(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            rs485agent = new RS485Agent(portName, baudRate, dataBits, stopBits, parity);
            rs485agent.OnDataReceived += Rs485DataReceived;
            rs485agent.Start();
        }
        public void SetLogger(IProcLogger logger)
        {
            this.logger = logger;
            rs485agent.SetLogger(logger);
        }
        public RS485Cytel CreateRS485Cytel(byte address)
        {
            RS485Cytel cytel = new RS485Cytel(address, rs485agent);
            cytel.SetLogger(logger);
            cytelDict.Add(address, cytel);

            return cytel;
        }
        private void Rs485DataReceived(byte[] data)
        {
            logger.Debug($"Rs485DataReceived: {BitConverter.ToString(data)}");
            int addr = (data[1] & 0x0F) * 10 + data[2] & 0x0F;
            RS485Cytel cytel = cytelDict[addr];
            cytel.DataReceiving(data);
        }

        ~Manager()
        {
            rs485agent.Close();
            logger.Debug("RS485 closed.");
        }
    }
}
