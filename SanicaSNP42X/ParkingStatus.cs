using System;
using System.Collections.Generic;
using System.Text;

namespace SanicaSNP42X
{
    public class ParkingStatus
    {
        internal int address;
        public int Address { get { return address; } }
        internal LoopSensorStatus loopSensorStatus;
        public LoopSensorStatus LoopSensorStatus { get { return loopSensorStatus; } }
        internal MatSwitchStatus matSwitchStatus;
        public MatSwitchStatus MatSwitchStatus { get { return matSwitchStatus; } }
        internal LockPlateStatus lockPlateStatus;
        public LockPlateStatus LockPlateStatus {  get { return lockPlateStatus; } }
        internal SensorStatus sensorStatus;
        public SensorStatus SensorStatus { get { return sensorStatus; } }
        internal int loopCount;
        public int LoopCount { get { return loopCount; } }
        internal int offBaseCount;
        public int OffBaseCount { get { return offBaseCount; } }
        internal int onLevelCount;
        public int OnLevelCount { get { return onLevelCount; } }
        internal int offLevelCount;
        public int OffLevelCount { get { return offLevelCount; } }
        internal int onBaseCount;
        public int OnBaseCount { get { return onBaseCount; } }
        internal ParkingStatus() { }

    }
}
