using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    public class Sum_Point
    {
        public float x;                 //当前x轴平均坐标
        public float y;                 //当前y轴平均坐标
        public long num;                //当前已经加和平均计算过的点数量

        public void set(float xpos,float ypos,long number)
        {
            x = xpos;
            y = ypos;
            num = number;
        }

        public void set(XYPoint point)
        {
            x = point.x;
            y = point.y;
            num = 1;
        }

        public void set(Sum_Point point)
        {
            x = point.x;
            y = point.y;
            num = point.num;
        }

        public void add(float xpos, float ypos, long number)
        {
            x = (x * num + xpos * number) / (num + number);
            y = (y * num + ypos * number) / (num + number);
            num = num + number;
        }

        public void add(XYPoint point)             //将point的值纳入平均计算中
        {
            x = (x * num + point.x) / (num + 1);
            y = (y * num + point.y) / (num + 1);
            num++;
        }

        public void add(Sum_Point point)           //将point的值纳入平均计算中
        {
            x = (x * num + point.x * point.num) / (num + point.num);
            y = (y * num + point.y * point.num) / (num + point.num);
            num = num + point.num;
        }
    }

    public class grid
    {
        public const int MIN_DISTANCE = 10;
        private int minNumber = 0;                    //视为物体的最小点数
        private int MaxDistance;                      //x轴和y轴范围最大值
        private int distance;                         //网格的边长
        private int maxPointNumber;                   //点数的最大值
        private Dictionary<long, long> sumKey;
        private Stack<long> keyStack;
        private Dictionary<long, Sum_Point> nowlist;
        private Dictionary<long, Sum_Point> pointGrid;
        private long tolindex;                        //maxdistance/distance（一行最大网格数）
        public List<XYPoint> pointList;

        public grid(int mindistance, int maxdistance, int maxNumber)
        {
            if (mindistance < MIN_DISTANCE)
                distance = MIN_DISTANCE;
            else
                distance = mindistance;
            MaxDistance = maxdistance;
            maxPointNumber = maxNumber;
            tolindex = (long)MaxDistance / (long)distance;
            sumKey = new Dictionary<long, long>(2 * maxPointNumber);
            nowlist = new Dictionary<long, Sum_Point>(2 * maxPointNumber);
            pointGrid = new Dictionary<long, Sum_Point>(2 * maxPointNumber);
            keyStack = new Stack<long>(2 * maxPointNumber);
        }

        public void clear()      //清空所有数据
        {
            sumKey.Clear();
            nowlist.Clear();
            keyStack.Clear();
            pointGrid.Clear();
        }

        public void add(XYPoint[] point)       //向网格中增加雷达数据
        {
            long x, y, key;
            Sum_Point sum;
            for (int i = 0; i < point.Length; i++) 
            {
                x = (long)point[i].x / distance;
                y = (long)point[i].y / distance;
                key = x * tolindex + y;
                if (!sumKey.ContainsKey(key))
                {
                    sum = new Sum_Point();
                    sum.set(point[i]);
                    pointGrid.Add(key, sum);
                    sumKey.Add(key, key);
                    keyStack.Push(key);
                }
                else
                {
                    sum = pointGrid[key];
                    sum.add(point[i]);
                    pointGrid[key] = sum;
                }
            }
        }

        private void setSumKey()           //为每个网格设置物体归属（即属于一个物体的网格ID相同）
        {
            Stack<long> stack = new Stack<long>();
            long key, number, n, i, j;
            while (keyStack.Count != 0)
            {
                number = keyStack.Pop();
                stack.Clear();
                stack.Push(number);
                while (stack.Count != 0)
                {
                    n = stack.Pop();
                    for (i = -1; i <= 1; i++)
                        for (j = -1; j <= 1; j++)
                        {
                            key = n + i * tolindex + j;
                            if (sumKey.ContainsKey(key))
                                if (sumKey[key] != sumKey[number])
                                {
                                    stack.Push(key);
                                    sumKey[key] = sumKey[number];
                                }
                        }
                }
            }
        }

        private void getDataFromNowlist()        //数据输出到pointList里
        {
            pointList = new List<XYPoint>();
            XYPoint point;
            foreach (long k in nowlist.Keys)
                if (nowlist[k].num > minNumber)
                {
                    point.x = nowlist[k].x;
                    point.y = nowlist[k].y;
                    pointList.Add(point);
                }
        }

        void sumPoint()              //计算点的平均值
        {
            long key;
            Sum_Point xp, yp;
            foreach (long k in pointGrid.Keys)
            {
                yp = pointGrid[k];
                key = sumKey[k];
                if (nowlist.ContainsKey(key))
                {
                    xp = nowlist[key];
                    xp.add(yp);
                    nowlist[key] = xp;
                }
                else
                {
                    xp = new Sum_Point();
                    xp.set(yp);
                    nowlist.Add(key, xp);
                }
            }
        }

        public void combine()    //SetSumKey、sumPoint、getDataFromNowList和clear操作的整合
        {
            setSumKey();
            sumPoint();
            getDataFromNowlist();
            clear();
        }
    }
}
//Grid类使用方法：clear()->为每个雷达的xypoint数据调用add->combine()->pointList获取数据