using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WebSimulateSNP42X.Model;
using SNP42XSimulator;

namespace WebSimulateSNP42X.Services
{
    public class ParkingLotService
    {
        private IWorkLogger _logger = new WorkLogger();
        private ParkingLotAgent _agent;
        private ParkingLotOverview _parkingLotOverview;

        public ParkingLotService() 
        {
            _logger.Info("INIT ParkingLotService.");

            Settings settings = Settings.INSTANCE;

            _agent = ParkingLotAgent.getInstance(settings.ComportName);

            _parkingLotOverview = new ParkingLotOverview() { Spaces = 0 };
        }
        [ResourceMethod(RequestMethod.GET)]
        public ParkingLotOverview Overview()
        {
            _parkingLotOverview.Spaces = _agent.ParkingSpacesSize();

            return _parkingLotOverview;
        }
        [ResourceMethod(RequestMethod.PUT, "addParkingSpace")]
        public Result AddParkingSpace(ParamAddr parkingSpace)
        {
            try
            {
                int addr = int.Parse(parkingSpace.Address);

                if(addr < 0)
                {
                    _logger.Error("ParkingSpace address should be positive integer.");
                    return new Result(0x02, "ParkingSpace address should be positive integer.");
                }

                _agent.AddParkingSpace((ushort)addr);
                _logger.Info($"ParkingSpace has added as address #{addr}.");
                return new Result(0x00, "");
            }
            catch (SNP42XSimulatorException ex)
            {
                return new Result(ex.ErrCode, "Fail to add parking space.");
            }
            catch (Exception ex)
            {
                return new Result(0x01, ex.Message);
            }
        }
        [ResourceMethod(RequestMethod.POST, "start")]
        public Result Start()
        {
            _logger.Info("in RequestMethod.POST start");
            try {
                _agent.Start();
                _logger.Info("ParkingLot Agent has started.");
                return new Result(0x00, "ParkingLot Agent has started.");
            }
            catch(Exception ex)
            {
                return new Result(0x03, ex.Message);
            }
        }
        [ResourceMethod(RequestMethod.POST, "stop")]
        public Result Stop()
        {
            try
            {
                _agent.Stop();
                _logger.Info("ParkingLot Agent has stopped.");
                return new Result(0x00, "ParkingLot Agent has stopped.");
            }
            catch (Exception ex)
            {
                return new Result(0x04, ex.Message);
            }
        }
        [ResourceMethod(RequestMethod.POST, "setLoopSensorON")]
        public Result SetLoopSensorON(ParamAddr parkingSpace)
        {
            return SetLoopSensor(parkingSpace, LoopSensorStatus.ON);
        }
        [ResourceMethod(RequestMethod.POST, "setLoopSensorOFF")]
        public Result SetLoopSensorOFF(ParamAddr parkingSpace)
        {
            return SetLoopSensor(parkingSpace, LoopSensorStatus.OFF);
        }
        [ResourceMethod(RequestMethod.POST, "setLoopSensorERROR")]
        public Result SetLoopSensorERROR(ParamAddr parkingSpace)
        {
            return SetLoopSensor(parkingSpace, LoopSensorStatus.Error);
        }
        [ResourceMethod(RequestMethod.POST, "setLoopSensorNA")]
        public Result SetLoopSensorNA(ParamAddr parkingSpace)
        {
            return SetLoopSensor(parkingSpace, LoopSensorStatus.NA);
        }
        private Result SetLoopSensor(ParamAddr parkingSpace, LoopSensorStatus status)
        {
            try
            {
                int addr = int.Parse(parkingSpace.Address);

                if (addr < 0)
                {
                    _logger.Error("ParkingSpace address should be positive integer.");
                    return new Result(0x02, "ParkingSpace address should be positive integer.");
                }

                _agent.SetLoopSensorStatus((ushort)addr, status);

                _logger.Info($"ParkingSpace #{addr} Loop Sensor has been {status.ToString()}.");

                return new Result(0x00, $"Loop sensor {status.ToString()}.");
            }
            catch (SNP42XSimulatorException ex)
            {
                return new Result(ex.ErrCode, $"Fail to set Loop Sensor {status.ToString()} for ParkingSpace #{parkingSpace.Address}");
            }
            catch (Exception ex)
            {
                return new Result(0x01, ex.Message);
            }
        }
    }
}
