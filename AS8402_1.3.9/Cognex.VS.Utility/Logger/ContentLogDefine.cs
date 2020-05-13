using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.VS.Utility
{
    public static class GlobalVariable
    {
        public static string DateTimeFormat = "yyyyMMdd HH:mm:ss.ffff";
        public static string HeaderContentPatten = "########################AUTO GENERATED DO NOT DELETE##########################";
    }

    [Serializable]
    public enum UserType
    {
        Unknown,

        Administrator,
        Engineer,
        Operator,

        NumberUserType
    }

    [Serializable]
    public enum MessageType
    {
        Unknown,
        Inform,
        Alarm,
        NumberMessageType
    }

    [Serializable]
    public enum InformSeverity
    {
        Unknown,
        Inform,
        Warning,
        Error,
        NumberInfoSeverity
    }

    [Serializable]
    public enum AlarmSeverity
    {
        Unknown = 0,
        CameraHardwareError = 1,
        ToolblockLoadError = 2,
        CameraAcqFailure = 3,
        CalibrationParamError,
        InspectionError,
        InspectionMissing,
        HardDiskFull,
        OutOfMemory,
        NumberAlarmSeverity
    }
}
