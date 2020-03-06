using System;
using System.Runtime.InteropServices;

namespace Radar
{   //A2雷达有两种扫描模式，分别为startScan(defaultScan)和startScanExpress(defaultScanExpress)
    //分别为普通测距模式和高速测距模式，每个模式内部又可以用rplidarScanMode进行调整
    //下面为A2雷达支持的三种rplidarScanMode。
    //AllscanMode  0 500 12 129 Standard
    //             1 250 12 130 Express
    //             2 125 12 132 Boost

    public enum ConnectType : UInt32
    {
        DRIVER_TYPE_SERIALPORT = 0x0,
        DRIVER_TYPE_TCP = 0x1
    }

    public enum TIMEOUT : UInt32
    {
        DEFAULT_TIMEOUT = 2000
    }

    public enum result : UInt32
    {
        RESULT_OK = 0,
        RESULT_FAIL_BIT = 0x80000000,
        RESULT_ALREADY_DONE = 0x20,
        RESULT_INVALID_DATA = (0x8000 | RESULT_FAIL_BIT),
        RESULT_OPERATION_FAIL = (0x8001 | RESULT_FAIL_BIT),
        RESULT_OPERATION_TIMEOUT = (0x8002 | RESULT_FAIL_BIT),
        RESULT_OPERATION_STOP = (0x8003 | RESULT_FAIL_BIT),
        RESULT_OPERATION_NOT_SUPPORT = (0x8004 | RESULT_FAIL_BIT),
        RESULT_FORMAT_NOT_SUPPORT = (0x8005 | RESULT_FAIL_BIT),
        RESULT_INSUFFICIENT_MEMORY = (0x8006 | RESULT_FAIL_BIT),
        RESULT_OPERATION_ABORTED = (0x8007 | RESULT_FAIL_BIT),
        RESULT_NOT_FOUND = (0x8008 | RESULT_FAIL_BIT),
        RESULT_RECONNECTING = (0x8009 | RESULT_FAIL_BIT)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct rplidar_response_measurement_node_hq_t
    {
        public UInt16 angle_z_q14;
        public UInt32 dist_mm_q2;
        public Byte quality;
        public Byte flag;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct rplidar_response_device_health_t
    {
        public Byte status;
        public UInt16 error_code;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct RplidarScanMode
    {
        public UInt16 id;
        public float us_per_sample;
        public float max_distance;
        public Byte ans_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] scan_mode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct rplidar_response_device_info_t
    {
        public Byte model;
        public UInt16 firmware_version;
        public Byte hardware_version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] serialnum;
    }

    public class originRadarA2
    {
        [DllImport(@"RPLIDAR.dll", EntryPoint = "CreateDriver", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //创建一个对应的 RPlidarDriver 实例,与设备进行通讯，完成操作后，需要调用dispose释放内存
        public extern static IntPtr CreateDriver(uint drivertype = (UInt32)ConnectType.DRIVER_TYPE_SERIALPORT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "DisposeDriver", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //释放雷达的内存
        public extern static void DisposeDriver(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "driverconnect", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 driverconnect(IntPtr drv, char[] portPath, UInt32 baudrate, UInt32 flag = 0);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "disconnect", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static void disconnect(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "isConnected", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool isConnected(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "reset", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 reset(IntPtr drv, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "clearNetSerialRxCache", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 clearNetSerialRxCache(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getTypicalScanMode", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getTypicalScanMode(IntPtr drv, ref UInt16 outMode, UInt32 timeoutInMs = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "startScan", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 startScan(IntPtr drv, bool force, bool useTypicalScan, UInt32 options, ref RplidarScanMode outUsedScanMode);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "defaultScan", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 defaultScan(IntPtr drv, bool force, bool useTypicalScan, UInt32 options = 0);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getHealth", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getHealth(IntPtr drv, ref rplidar_response_device_health_t health, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getDeviceInfo", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getDeviceInfo(IntPtr drv, ref rplidar_response_device_info_t info, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "setMotorPWM", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 setMotorPWM(IntPtr drv, UInt16 pwm);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "startMotor", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 startMotor(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "stopMotor", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 stopMotor(IntPtr drv);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "checkMotorCtrlSupport", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 checkMotorCtrlSupport(IntPtr drv, ref bool support, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getFrequency", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getFrequency(IntPtr drv, ref RplidarScanMode scanMode, UInt32 count, ref float frequency);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "startScanNormal", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 startScanNormal(IntPtr drv, bool force, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "stop", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 stop(IntPtr drv, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "grabScanDataHq", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 grabScanDataHq(IntPtr drv, ref rplidar_response_measurement_node_hq_t nodebuffer, ref UInt32 count, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "ascendScanData", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 ascendScanData(IntPtr drv, ref rplidar_response_measurement_node_hq_t nodebuffer, UInt32 count);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getScanDataWithIntervalHq", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getScanDataWithIntervalHq(IntPtr drv, ref rplidar_response_measurement_node_hq_t nodebuffer, ref UInt32 count);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "startScanExpress", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 startScanExpress(IntPtr drv, bool force, UInt16 scanMode, UInt32 options, ref RplidarScanMode outUsedScanMode, UInt32 timeout);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "defaultScanExpress", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 defaultScanExpress(IntPtr drv, bool force, UInt16 scanMode, UInt32 options = 0, UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);

        [DllImport(@"RPLIDAR.dll", EntryPoint = "getAllSupportedScanModes", CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public extern static UInt32 getAllSupportedScanModes(IntPtr drv, ref UInt32 count, UInt32 timeoutInMs = (UInt32)TIMEOUT.DEFAULT_TIMEOUT);
    }
}
