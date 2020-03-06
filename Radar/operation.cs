using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Radar
{
    class way
    {
        private static SingleMessage single = new SingleMessage();    //单个消息获取
        private static MultiMessage multi = new MultiMessage();       //多个消息打包
        private static DateTime time = DateTime.Now;                  //现在时间
        public static float dtime;                                    //更新间隔时间
        //Operation先判断时间流逝是否大于更新间隔时间，只有大于时间间隔才执行下面操作
        //将每个雷达获取的xy坐标点数据添加到pointGrid中，启用combine方法合并点数据后，传入到management进行鼠标信息的更新
        //然后对cursor2Dset里的每一个鼠标用single生成set信息，添加到multi里
        //对所有鼠标用single生成alive信息并添加到multi里
        //用frame和single生成fseq信息，添加到multi里
        //最后用udpclient传递数据到客户端，并把frame加1。
        public static void operation(ref List<Radar> radar, grid pointGrid, Cursor2DManagement management, ref UdpClient x, ref int frame, int maxnumber)
        {
            if ((DateTime.Now - time).TotalMilliseconds >= dtime)
            {
                frame++;
                time = DateTime.Now;
                pointGrid.clear();
                for (int i = 0; i < radar.Count; i++)
                {
                    radar[i].xyUpdate(maxnumber);
                    radar[i].cutPointXY();
                    pointGrid.add(radar[i].getXYdata());
                }
                pointGrid.combine();
                management.update(ref pointGrid.pointList);
                multi.clear();

                for (int i = 0; i < management.cursor2Dset.Count; i++)
                {
                    single.setCur2DMessage(management.cursor2Dset[i]);
                    multi.add(single);
                }
                int[] alive = new int[management.cursor2Dset.Count];
                for (int i = 0; i < management.cursor2Dset.Count; i++)
                    alive[i] = management.cursor2Dset[i].ID();
                single.aliveCur2DMessage(ref alive);
                multi.add(single);
                single.fseqCur2DMessage(frame);
                multi.add(single);
                byte[] message = multi.getMessage();
                x.Send(message, message.Length);
            }
        }
    }
}
