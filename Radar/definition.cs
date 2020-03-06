using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    public struct radarConnectType        //雷达连接方式定义
    {
        public ConnectType connectType;   //连接方式，DRIVER_TYPE_TCP为网口连接，DRIVER_TYPE_SERIALPORT为端口连接
        public string serialPort;         //表示端口地址，如“192.168.0.7”（网口连接）和“COM3”（端口连接）
        public UInt32 serialBaudrate;     //端口波特率
        public int maxCOM;                //最大端口频道数

        ////预置方法定义（提供常用的连接方式，简化参数的设置）
        public void setA2defaultTCP()     //A2雷达TCP连接
        {
            this.connectType = ConnectType.DRIVER_TYPE_TCP;
            this.serialPort = "192.168.0.7";
            this.serialBaudrate = 20108;
            this.maxCOM = 16;
        }

        public void setA2defaultSerial()  //A2雷达端口连接
        {
            this.connectType = ConnectType.DRIVER_TYPE_SERIALPORT;
            this.serialPort = "COM3";
            this.serialBaudrate = 115200;
            this.maxCOM = 16;
        }

        public void setG4defaultSerial()  //G4雷达端口连接
        {
            this.connectType = ConnectType.DRIVER_TYPE_SERIALPORT;
            this.serialPort = "COM3";
            this.serialBaudrate = 230400;
            this.maxCOM = 16;
        }
    }

    public struct RadarPosition           //雷达位置定义
    {
        public int radarPositionX;        //雷达x轴位置（mm为单位）
        public int radarPositionY;        //雷达y轴位置（mm为单位）
        public int radarTheta;            //雷达角度（角度为单位）
        public void set(int x,int y,int theta)
        {
            this.radarPositionX = x;
            this.radarPositionY = y;
            this.radarTheta = theta;
        }
    }

    public class RadarSetting             //雷达设定参数的整合
    {
        public RadarPosition position;    //雷达位置信息
        public Area area;                 //雷达检测范围
        public radarConnectType type;     //雷达连接方式
        public int capacity;              //雷达一次扫描最大点数
    }
}
