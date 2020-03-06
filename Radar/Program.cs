using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace Radar
{
    public delegate bool ControlCtrlDelegate(int CtrlType);
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);
        public static bool HandlerRoutine(int CtrlType)
        {
            for (int i=0;i<ProgramRadars.Count;i++)
                ProgramRadars[i].close();
            return false;
        }  //窗口退出时关闭所有的雷达

        static List<Radar> ProgramRadars;  //启用的雷达集合

        static List<Radar> radarSet(int maxnumber)  //设置可使用的雷达，其中maxnumber为单个雷达单次最大采样点数量
        {
            List<Radar> set = new List<Radar>();

            RadarSetting setting = new RadarSetting();  //雷达设置，area为雷达涵盖的区域坐标，capacity为雷达点的容量，
            setting.area = new squareArea(1780, 0, 3560, 2000); //position为雷达的xy坐标，type为雷达的链接方式
            setting.capacity = maxnumber;                       //链接方式有TCP链接和串口链接，有默认的函数进行设置，当然也可以根据
            setting.position.set(2670, 2300, -180);             //需要手动设置
            setting.type.setA2defaultTCP();

            RadarA2 radar1 = new RadarA2(setting);              //设置A2雷达
            radar1.connect();                                   //连接雷达
            if (radar1.isHealthAndConnect())                    //若连接成功，加入可用雷达序列
                set.Add(radar1);


            setting.type.setG4defaultSerial();
            setting.position.set(895, 2300, -180);
            setting.area = new squareArea(0, 0, 1790, 2000);
            RadarG4 radar2 = new RadarG4(setting);
            radar2.connect();
            if (radar2.isHealthAndConnect())
                set.Add(radar2);
            ProgramRadars = set;
            return set;                                         //返回可用雷达集合
        }

        static void Main(string[] args)
        {
            bool bRet = SetConsoleCtrlHandler(newDelegate, true);
            UdpClient x = new UdpClient(0);
            x.Connect("localhost", 3333);                       //将雷达的数据通过udp的方式传给应用，默认端口为3333

            int oneRadarMaxnumber = 3000;                       //设置单个雷达单次采样的最大点数
            List<Radar> radars = radarSet(oneRadarMaxnumber);

            Cursor2DManagement management = new Cursor2DManagement();  //管理通过雷达生成的触摸点
            int mergeDistance = 50, maxX = 3560, maxY = 2000;          //maxX和maxY分别为屏幕坐标的最大距离（最终传输数据限制在0到1之间）
                                                                       //mergeDistance为两个点融合为一个点的最小距离
                                                                       //（因为物体有体积，一个物体往往在雷达里会生成多个靠近的点）
            grid pointGrid = new grid(mergeDistance, Math.Max(maxX, maxY), oneRadarMaxnumber * radars.Count);
            Cursor2D.set(maxX, maxY);                                  //设置融合操作

            int frame = 0;
           
            way.dtime = 1000.0f / 60.0f;                               //雷达数据的更新频率，为60帧每秒
            for (int i = 0; i < radars.Count; i++)
                radars[i].start();                                     //雷达开启扫描

            while (true)                                               //雷达获取数据和数据的操作
                way.operation(ref radars, pointGrid, management, ref x, ref frame, oneRadarMaxnumber);

            for (int i=0;i<radars.Count;i++)
                radars[i].close();
        }
    }
}
