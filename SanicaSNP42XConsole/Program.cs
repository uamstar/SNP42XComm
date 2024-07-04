using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Timers;
using System.CommandLine;
using SanicaSNP42X;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.Tracing;
using NLog;

namespace SanicaSNP42XConsole
{
    internal class Program
    {
        private const string VER = "0.1";
        private static IProcLogger logger = new RunningLogger();
        private static RS485Cytel cytel1;
        private const int POLLING_INTERVAL = 1000;   // 每 1 秒發送一次
        private const int BLINK_INTERVAL = 500;   // 每 0.7 秒閃爍一次

        private static int _pollingCounter = 0;

        private static bool _doExit = false;

        static async Task<int> Main(string[] args)
        {
            var option = new Option<string?>(
            name: "--port",
            description: "Serial port name, ex: COM1, /dev/ttys002, ...");

            var addrOption = new Option<int>(
            name: "--address",
            description: "Address of SNP42X parking lock",
            getDefaultValue: () => 1);

            var rootCommand = new RootCommand("Sample console for demonstrating SanicaSNP42X AIP.")
            {
                option,
                addrOption
            };

            rootCommand.SetHandler((port, address) =>
            {
                LoopSensorStatus _preLoopSensorStatus;
                LockPlateStatus _preLockPlateStatus;
                SensorStatus _preSensorStatus;

                Console.WriteLine($"SanicaSNP42X Console {VER}");
                Console.WriteLine($"Use {port}, SNP42X address {address}");

                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                strExeFilePath = strExeFilePath.Substring(0, strExeFilePath.LastIndexOf(Path.DirectorySeparatorChar));
                string settingsFilePath = Path.Combine(strExeFilePath, "appsettings.json");

                Console.WriteLine($"Use settings file: {settingsFilePath}");

                IConfigurationRoot config = new ConfigurationBuilder()
                    .AddJsonFile(settingsFilePath)
                    .AddEnvironmentVariables()
                    .Build();

                // Get values from the config given their key and their target type.
                try
                {
                    Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

                    StopBits stopBits;
                    if (settings.Comport.StopBits == 1)
                        stopBits = StopBits.One;
                    else if (settings.Comport.StopBits == 2)
                        stopBits = StopBits.Two;
                    else stopBits = StopBits.None;

                    Manager manager = Manager.getInstance(
                        port,
                        settings.Comport.BaudRate,
                        (Parity)Enum.Parse(typeof(Parity), settings.Comport.Parity),
                        settings.Comport.DataBits,
                        stopBits);

                    manager.SetLogger(logger);

                    cytel1 = manager.CreateRS485Cytel((byte)address);
                    cytel1.ParkingPlateStatusResponse += PSRespReceivied;
                    cytel1.ParkingControlResponse += PCRespReceivied;
                    cytel1.NoResponse += NoRespReceivied;

                    ShowCommandList();

                    while (!_doExit) 
                    {
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Fail to get settings from appsettings.json.", ex);
                }

            }, option, addrOption);

            return await rootCommand.InvokeAsync(args);
        }
        static void ShowUserInterface()
        {
            string val;
            char c = '\0';
            Console.WriteLine();
            Console.Write("Enter (1~6, c, x): ");
            val = Console.ReadLine();

            if (!val.Equals(""))
            {
                c = val[0];
                switch (c)
                {
                    case '1':
                        Polling();
                        break;
                    case '2':
                        GetPlateStatus();
                        break;
                    case '3':
                        LoopSensorOn();
                        break;
                    case '4':
                        LoopSensorOff();
                        break;
                    case '5':
                        LockingCtrlUp();
                        break;
                    case '6':
                        LockingCtrlDown();
                        break;
                    case 'c':
                    case 'C':
                        ShowCommandList();
                        break;
                    case 'x':
                    case 'X':
                        Console.WriteLine("Bye!");
                        _doExit = true;
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        ShowUserInterface();
                        break;
                }
            }
            else ShowUserInterface();
        }
        static void ShowCommandList()
        {
            Console.WriteLine("Command List: (Input 1~6, c, x)");
            Console.WriteLine("Send Command:");
            Console.WriteLine("\t1) Polling");
            Console.WriteLine("\t2) Get Plate Status");
            Console.WriteLine("\t3) Set Loop Sensor On");
            Console.WriteLine("\t4) Set Loop Sensor Off");
            Console.WriteLine("\t5) Set Locking Ctrl Up");
            Console.WriteLine("\t6) Set Locking Ctrl Down");
            Console.WriteLine("c) Show Command List");
            Console.WriteLine("x) Exit");
            ShowUserInterface();
        }
        static void PSRespReceivied(ParkingStatus ps)
        {
            LogOutput($"Receivied: Addr {ps.Address}");
            LogOutput($"\tLoop Sensor Status: {ps.LoopSensorStatus}");
            LogOutput($"\tMat Switch Status: {ps.MatSwitchStatus}");
            LogOutput($"\tLock Plate Status: {ps.LockPlateStatus}");
            LogOutput($"\tSensor Status: {ps.SensorStatus}");
            LogOutput($"\tLoop count: {ps.LoopCount}");
            LogOutput($"\tBase count: ON: {ps.OnBaseCount}, OFF: {ps.OffBaseCount}");
            LogOutput($"\tLevel count: ON: {ps.OnLevelCount}, OFF: {ps.OffLevelCount}");

            ShowUserInterface();
        }
        static void PCRespReceivied(PC_Resp pcResp)
        {
            LogOutput($"Parking Plate Control Result: {pcResp}");
            ShowUserInterface();
        }
        static void NoRespReceivied(RS485Cytel sender)
        {
            LogOutput($"no response from #{sender.Address} plate.");
            ShowUserInterface();
        }
        static void GetPlateStatus()
        {
            LogOutput($"Addr#{cytel1.Address}: Send GetPlateStatus command.");
            cytel1.GetLockingPlateStatus();
        }
        static void LoopSensorOn()
        {
            LogOutput($"Addr#{cytel1.Address}: Send TurnLoopOn command.");
            cytel1.TurnLoopOn();
        }
        static void LoopSensorOff()
        {
            LogOutput($"Addr#{cytel1.Address}: Send TurnLoopOff command.");
            cytel1.TurnLoopOff();
        }
        static void LockingCtrlUp()
        {
            LogOutput($"Addr#{cytel1.Address}: Send TurnLockUp command.");
            cytel1.TurnLockUp();
        }
        static void LockingCtrlDown()
        {
            LogOutput($"Addr#{cytel1.Address}: Send TurnLockDown command.");
            cytel1.TurnLockDown();
        }
        static void Polling()
        {
            LogOutput($"Addr#{cytel1.Address}: Send Polling command.");
            cytel1.Polling();
        }
        static void LogOutput(string msg)
        {
            logger.Info(msg);
            Console.WriteLine(msg);
        }
    }
}
