using System;
using System.Collections.Generic;
using System.Text;

namespace SNP42XSimulator
{
    public enum LoopSensorStatus
    {
        NA = 0x30,
        OFF = 0x31,
        ON = 0x32,
        Error = 0x33,
        ForcedOFF = 0x34,
        ForcedON = 0x35
    }

    public enum MatSwitchStatus
    {
        NA = 0x30,
        OFF = 0x31,
        ON = 0x32
    }

    public enum LockPlateStatus
    {
        StandBy = 0x30,
        Declining = 0x31,
        Inclining = 0x32,
        ErrInDecline = 0x33,
        ErrInIncline = 0x34,
        ForcedDecline = 0x35,
        ForcedIncline = 0x36,
        IrregalLocking = 0x3D
    }

    public enum SensorStatus
    {
        Middle = 0x30,
        BottomEnd = 0x31,
        TopEnd = 0x32,
        Error = 0x33
    }

}
