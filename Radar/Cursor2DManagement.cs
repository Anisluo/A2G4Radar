using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    public class Cursor2DManagement
    {
        public List<Cursor2D> cursor2Dset;                 //当前存在的鼠标集合
        public List<Cursor2D> newCursor;                   //新添加的鼠标集合
        private SIDManagement IDprovide;                   //s_ID管理器
        private float distance;                            //最小识别距离（毫米为单位）
        private DateTime time = DateTime.MinValue;         //当前时间

        public Cursor2DManagement(float dist = 100)
        {
            IDprovide = new SIDManagement();
            distance = dist;
            newCursor = new List<Cursor2D>();
            cursor2Dset = new List<Cursor2D>();
            clear();
        }

        public void clear()                        //清除掉Cursor2Dset的数据
        {
            cursor2Dset.Clear();
        }

        private float getTime()                    //获取流逝的时间（秒为单位）
        {
            if (time == DateTime.MinValue)
                time = DateTime.Now;
            float dTime = (float)(DateTime.Now - time).TotalMilliseconds / 1000.0f;
            time = DateTime.Now;
            return dTime;
        }

        public void update(ref List<XYPoint> currentPoint)    //利用currentPoint里的数据更新Cursor2Dset里的鼠标
        {
            float dTime = getTime();
            int i, j;
            bool[] if_have_point = new bool[currentPoint.Count];
            bool[] if_have_cursor = new bool[cursor2Dset.Count];
            float x, y;
            for (i = 0; i < currentPoint.Count; i++) if_have_point[i] = false;
            for (i = 0; i < if_have_cursor.Length; i++) if_have_cursor[i] = false;


            for (i = 0; i < cursor2Dset.Count; i++)
            {
                x = cursor2Dset[i].xPos() + cursor2Dset[i].xSpeed() * dTime + cursor2Dset[i].xAccel() * dTime * dTime;
                y = cursor2Dset[i].yPos() + cursor2Dset[i].ySpeed() * dTime + cursor2Dset[i].yAccel() * dTime * dTime;
                for (j = 0; j < currentPoint.Count; j++)
                {
                    if ((Math.Abs(x - currentPoint[j].x) < distance) && (Math.Abs(y - currentPoint[j].y) < distance) && (!if_have_point[j]))
                    {
                        if_have_cursor[i] = true;
                        cursor2Dset[i].update(currentPoint[j]);
                        if_have_point[j] = true;
                        break;
                    }
                }
            }//每个cursor2D对象根据当前位置、流逝时间、速度和加速度计算预估位置，公式为pos + speed * dtime + accel * dtime * dtime
             //然后在以这个位置为中心，半径为distance的圆内查找是否存在currentPoint内的点
            //以第一个匹配到的点作为cursor2D最新的位置。如果没有匹配的点，则删除这个cursor2D对象。

            newCursor.Clear();
            for (i = 0; i < cursor2Dset.Count; i++)
                if (!if_have_cursor[i])
                    IDprovide.freeID(cursor2Dset[i].ID());
                else
                    newCursor.Add(cursor2Dset[i]);
            //最后没有与任何cursor2D匹配的currentPoint里的点作为新生成的cursor2D的位置
            cursor2Dset.Clear();
            for (i = 0; i < newCursor.Count; i++)
                cursor2Dset.Add(newCursor[i]);

            newCursor.Clear();
            Cursor2D m;
            for (i = 0; i < currentPoint.Count; i++)
                if (!if_have_point[i])
                {
                    m = new Cursor2D(currentPoint[i].x, currentPoint[i].y, 0.0f, 0.0f, 0.0f, 0.0f);
                    m.setID(IDprovide.getID());
                    cursor2Dset.Add(m);
                    newCursor.Add(m);
                }
        }
    }
}
