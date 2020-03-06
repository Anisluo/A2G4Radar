using System;
using System.Runtime.InteropServices;

namespace Radar
{
    public struct XYPoint
    {
        public float x;			/*	坐标, 雷达点云在设定识别区域中的坐标 */
        public float y;
    };

    //G4雷达原始接口，具体配置信息可以参考相应文档
    public class originRadarG4
    {
        [DllImport(@"ydlidar_driver.dll", EntryPoint = "EAIScreenLd_init", CharSet = CharSet.Ansi,
     ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //G4雷达初始化，ulrsetting为设置文件的名称
        public extern static IntPtr EAIScreenLd_init(char[] ulrSetting);

        [DllImport(@"ydlidar_driver.dll", EntryPoint = "EAIScreenLd_getLdMap", CharSet = CharSet.Ansi,
            ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //获取xy坐标形式的点数据，ptArr为点数组，nPtArr为返回的有效点数量，调用时为点最大数量
        public extern static int EAIScreenLd_getLdMap(IntPtr handle, ref XYPoint ptArr, ref int nPtArr);

        [DllImport(@"ydlidar_driver.dll", EntryPoint = "EAIScreenLd_getDeviceInfo", CharSet = CharSet.Ansi,
            ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //获取G4雷达信息
        public extern static int EAIScreenLd_getDeviceInfo(IntPtr hhandle, byte[] seviceInfo);

        [DllImport(@"ydlidar_driver.dll", EntryPoint = "EAIScreenLd_getDeviceCom", CharSet = CharSet.Ansi,
            ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //获取设备的COM接口信息
        public extern static int EAIScreenLd_getDeviceCom(IntPtr hhandle, byte[] seviceCom);

        [DllImport(@"ydlidar_driver.dll", EntryPoint = "EAIScreenLd_destory", CharSet = CharSet.Ansi,
            ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //传递指针，释放雷达资源
        public extern static int EAIScreenLd_destory(ref IntPtr hhandle);
    }
}
