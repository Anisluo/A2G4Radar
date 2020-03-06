using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Radar
{
    abstract class Radar
    {
        protected IntPtr handle;                     //雷达对象指针
        public RadarPosition radarPosition;          //雷达坐标位置
        protected Area radarArea;                    //雷达检测范围

        protected XYPoint[] xyData;                  //雷达点数组（XY坐标）
        protected int dataNumber;                    //XyData数组里的有效点数

        protected int capacity;                      //雷达单次扫描最大点数

        public radarConnectType connectType;         //雷达连接方式
                                                     //为了节省空间和申请空间所需要的时间，xydata在雷达初始化时便申请capacity大小的空间
                                                     //后面不会改变，有效数据的大小通过dataNumber来反映。
        public XYPoint[] getXYdata()
        {
            XYPoint[] output = new XYPoint[dataNumber];          //返回当前所存储的点数据，函数内进行数组拷贝
            for (int i = 0; i < dataNumber; i++)                 //使返回数组的大小为dataNUmber
                output[i] = xyData[i];
            return output;                                 
        }

        public bool ifHaveHandle ()                              //Handle为空返回false，否则返回true
        {
            if (handle != IntPtr.Zero)
                return true;
            return false;
        }

        public void cutPointXY()                                 //根据radarArea裁剪不在范围内的点数据，并跟新dataNumber的有效值
        {
            int i, nowpotision = 0;
            for (i = 0; i < dataNumber; i++)
            {
                if (radarArea.ifInAreaXY(ref xyData[i]))
                {
                    xyData[nowpotision] = xyData[i];
                    nowpotision++;
                }
            }
            dataNumber = nowpotision;
        }

        abstract public void xyUpdate(int num);                 //获取雷达扫描一圈的数据，最大数量为num，数据为xy坐标
        abstract public void close();                           //关闭雷达连接并删除雷达，将handle置为空
        abstract public bool createNewRadar();                  //获取新的雷达指针handle
        abstract public bool isHealthAndConnect();              //检查雷达状态是否健康和雷达是否连接正常
        abstract public bool connect();                         //连接雷达
        abstract public bool start();                           //雷达开始扫描（包括开始旋转）
    }

    class RadarG4 : Radar
    {
        public int sampleRate;                       //雷达采集率
        public int scanFreq;                         //雷达扫描频率
        public bool ifReversion;                     //雷达方向是否反转
        private string filePath = "setting.ini";     //初始化文件地址

        public RadarG4(RadarSetting setting, string path = "setting.ini", int rate = 15, int freq = 15, bool reversion = false)
        {
            sampleRate = rate;
            scanFreq = freq;
            ifReversion = reversion;
            filePath = path;
            radarPosition = setting.position;
            radarArea = setting.area;
            connectType = setting.type;
            capacity = setting.capacity;
            createNewRadar();
            xyData = new XYPoint[capacity];
            dataNumber = 0;
        }

        public override bool start()                        //雷达开始扫描（包括开始旋转）
        {
            return true;
        }

        public override void xyUpdate(int num)              //获取雷达扫描一圈的数据，最大数量为num，数据为xy坐标
        {
            if (!isHealthAndConnect())
                connect();
            if (isHealthAndConnect())
            {
                if (num > capacity)
                    num = capacity;
                if (num < 0)
                    num = 0;
                int n = num;
                if (1 != originRadarG4.EAIScreenLd_getLdMap(handle, ref xyData[0], ref n))
                    n = 0;
                dataNumber = n;
            }
            else
                dataNumber = 0;
        }

        public override bool createNewRadar()                //获取新的雷达指针handle
        {
            writeInformation(filePath);
            handle = originRadarG4.EAIScreenLd_init(filePath.ToCharArray());
            if (IntPtr.Zero == handle)
            {
                Console.WriteLine("lidar init error !!! \n");
                return false;
            }
            return true;
        }

        public override bool connect()             //连接雷达
        {
            if (handle != IntPtr.Zero)
                return true;
            return false;
        }

        public override bool isHealthAndConnect()         //检查雷达状态是否健康和雷达是否连接正常
        {
            if (handle != IntPtr.Zero)
                return true;
            return false;
        }

        public override void close()                                        //关闭雷达连接并删除雷达，将handle置为空
        {
            if (handle != IntPtr.Zero)
                originRadarG4.EAIScreenLd_destory(ref handle);
            handle = IntPtr.Zero;
        }

        public string getDeviceCom()                                        //获取设备串口信息，如果没有连接雷达，返回“null”
        {
            if (handle!=IntPtr.Zero)
            {
                byte[] seviceCom = new byte[16];
                if (1 == originRadarG4.EAIScreenLd_getDeviceCom(handle, seviceCom))
                    return Encoding.UTF8.GetString(seviceCom);
            }
            return "null";
        }

        public string getDeviceInfo()                                       //获取设备信息，如果没有连接雷达，返回“null”
        {
            if (handle != IntPtr.Zero)
            {
                byte[] seviceInfo = new byte[16];
                if (1 == originRadarG4.EAIScreenLd_getDeviceInfo(handle, seviceInfo))
                    return Encoding.UTF8.GetString(seviceInfo);
            }
            return "null";
        }

        private void writeInformation(string settingPath)                   //将初始化信息写入文件中，待调用SDK接口初始化使用
        {
            squareBoundary overBoundary = radarArea.getSquareBoundary();
            FileStream file = new FileStream(settingPath, FileMode.Create, FileAccess.Write);
            StreamWriter write = new StreamWriter(file);
            write.WriteLine("[LiDarKeyName]");
            write.WriteLine("driverType={0}", (int)connectType.connectType);
            write.WriteLine("serialPort={0}", connectType.serialPort);
            write.WriteLine("serialBaudrate={0}", connectType.serialBaudrate);
            write.WriteLine("intensities={0}", 0);
            write.WriteLine("exposure={0}", 0);
            write.WriteLine("heartBeat={0}", 0);
            write.WriteLine("autoReconnect={0}", 0);
            write.WriteLine("sampleRate={0}", sampleRate);
            write.WriteLine("scanFreq={0}", scanFreq);
            write.WriteLine("MAX_COM={0}", connectType.maxCOM);
            write.WriteLine("[LiDarRangeKeyName]");
            write.WriteLine("Max_x={0}", overBoundary.xMax);
            write.WriteLine("Min_x={0}", overBoundary.xMin);
            write.WriteLine("Max_y={0}", overBoundary.yMax);
            write.WriteLine("Min_y={0}", overBoundary.yMin);
            write.WriteLine("[LiDarPoseKeyName]");
            write.WriteLine("pose_x={0}", radarPosition.radarPositionX);
            write.WriteLine("pose_y={0}", radarPosition.radarPositionY);
            write.WriteLine("pose_theta={0}", radarPosition.radarTheta);
            if (ifReversion)
                write.WriteLine("pose_reversion=1");
            else
                write.WriteLine("pose_reversion=0");
            write.Close();
            file.Close();
        }

        ~RadarG4()
        {
            this.close();
        }
    }

    class RadarA2 : Radar
    {
        public UInt32 timeout = (UInt32)TIMEOUT.DEFAULT_TIMEOUT;                    //最大延时
        private rplidar_response_measurement_node_hq_t[] nodeInformation;           //SDK接口返回的点数据

        public RadarA2(RadarSetting setting)
        {
            capacity = setting.capacity;
            connectType = setting.type;
            radarArea = setting.area;
            radarPosition = setting.position;
            xyData = new XYPoint[capacity];
            nodeInformation = new rplidar_response_measurement_node_hq_t[capacity];
            dataNumber = 0;
            createNewRadar();
        }

        public override bool createNewRadar()                                       //获取新的雷达指针handle
        {
            handle = originRadarA2.CreateDriver((UInt32)connectType.connectType);
            if (handle != IntPtr.Zero)
                return true;
            Console.WriteLine("Rplidar init error !!!");
            return false;
        }

        public rplidar_response_device_info_t getInfo()
        {
            rplidar_response_device_info_t information = new rplidar_response_device_info_t();
            originRadarA2.getDeviceInfo(handle, ref information, timeout);
            return information;
        }

        private void polarToxyChange()                                      //将SDK接口提供的极坐标数据根据定义转化为XY坐标数据并将数据传送给xyUpdate
        {
            float angle, distance, dx, dy;
            int nownumber = 0;
            for (int i = 0; i < dataNumber; i++) 
            {
                distance = (float)nodeInformation[i].dist_mm_q2 / 4.0f;
                if (distance > 0)
                {
                    angle = (float)nodeInformation[i].angle_z_q14 * 90.0f / (1 << 14) - 180.0f + radarPosition.radarTheta;
                    while (angle < 0.0f)
                        angle += 360.0f;
                    while (angle >= 360.0f)
                        angle -= 360.0f;
                    dx = distance * (float)Math.Cos(angle * Math.PI / 180.0f);
                    dy = distance * (float)Math.Sin(angle * Math.PI / 180.0f);
                    xyData[nownumber].x = radarPosition.radarPositionX + dx;
                    xyData[nownumber].y = radarPosition.radarPositionY + dy;
                    nownumber++;
                }
            }
            dataNumber = nownumber;
        }

        public override void xyUpdate(int num)                              //获取雷达扫描一圈的数据，最大数量为num，数据为xy坐标
        {
            if (handle != IntPtr.Zero)
            {
                if (num > capacity)
                    num = capacity;
                if (num <= 0)
                    num = 0;
                UInt32 count = (UInt32)num;
                originRadarA2.grabScanDataHq(handle, ref nodeInformation[0], ref count);
                dataNumber = (int)count;
                polarToxyChange();
            }
            else
                dataNumber = 0;
        }

        public bool stop()                                                  //停止A2雷达旋转和扫描
        {
            if (IntPtr.Zero == handle)
                return true;
            if (isOK(originRadarA2.stop(handle, timeout)))
                if (isOK(originRadarA2.stopMotor(handle)))
                    return true;
            return false;
        }

        public bool disconnect()                                            //断开A2雷达连接
        {
            if (IntPtr.Zero == handle)
                return true;
            if (stop())
            {
                originRadarA2.disconnect(handle);
                return true;
            }
            return false;
        }

        public int getAllScanMode()                         //获取A2雷达扫描模式的数量
        {
            uint count = 10;
            originRadarA2.getAllSupportedScanModes(handle, ref count, timeout);
            return (int)count;
        }

        public override bool start()                        //雷达开始扫描（包括开始旋转）
        {
            if ((IntPtr.Zero != handle) && (isOK(originRadarA2.startMotor(handle))) && isOK(originRadarA2.defaultScan(handle, false, true))) 
                return true;
            return false;
        }

        public override bool connect()                      //连接雷达
        {
            if ((IntPtr.Zero != handle) && isOK(originRadarA2.driverconnect(handle, connectType.serialPort.ToCharArray(), connectType.serialBaudrate)) &&isHealthAndConnect())
                return true;
            Console.WriteLine("Rplidar connnect error");
            return false;
        }

        public override bool isHealthAndConnect()                      //检查雷达状态是否健康和雷达是否连接正常
        {
            if (IntPtr.Zero != handle)
            {
                rplidar_response_device_health_t healthInformation = new rplidar_response_device_health_t();
                if (isOK(originRadarA2.getHealth(handle, ref healthInformation, timeout)))
                    if ((healthInformation.status == 0) && (originRadarA2.isConnected(handle)))
                        return true;
            }
            return false;
        }

        private bool isOK(UInt32 Result)                               //SDK接口提供的函数返回值都是事先定义好的宏，将结果传递到这个函数
        {                                                              //如果结果正常则返回True，如果出现异常返回false。
            if ((Result & (UInt32)result.RESULT_FAIL_BIT) == 0)        //如果要获取详细的异常信息，请参考SDK源码或SDK手册
                return true;
            return false;
        }

        public override void close()                                   //关闭雷达连接并删除雷达，将handle置为空
        {
            if (IntPtr.Zero != handle)
            {
                stop();
                originRadarA2.DisposeDriver(handle);
            }
            handle = IntPtr.Zero;
        }

        ~RadarA2()
        {
            close();
        }
    }
}
