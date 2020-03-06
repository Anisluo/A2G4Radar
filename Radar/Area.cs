using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    public struct squareBoundary  //定义了一个二维平面上的长方形范围
    {
        public float xMin;   //x轴最小值
        public float yMin;   //y轴最小值
        public float xMax;   //x轴最大值
        public float yMax;   //y轴最大值
    }

    public abstract class Area     //雷达检测范围抽象类
    {
        abstract public bool ifInAreaXY(ref XYPoint point);   //判断特定点是否在平面设定范围内
        abstract public squareBoundary getSquareBoundary();   //得到范围的外接长方形（长方形与坐标轴平行）
    }

    public class squareArea:Area   //范围为长方形
    {
        private squareBoundary boundary;

        public squareArea(float Xmin,float Ymin,float Xmax,float Ymax)
        {
            boundary.xMin = Xmin;
            boundary.yMin = Ymin;
            boundary.xMax = Xmax;
            boundary.yMax = Ymax;
        }

        public squareArea(squareBoundary Boundary)
        {
            boundary = Boundary;
        }

        public squareArea(ref squareArea area)
        {
            boundary = area.getSquareBoundary();
        }

        override public bool ifInAreaXY(ref XYPoint point)
        {
            if ((boundary.xMin < point.x && point.x < boundary.xMax) && (boundary.yMin < point.y && point.y < boundary.yMax))
                return true;
            return false;    
        }

        override public squareBoundary getSquareBoundary()
        {
            return boundary;
        }
    }
}
