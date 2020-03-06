using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    public class Cursor2D
    {
        private float xpos;                   //x轴位置
        private float ypos;                   //y轴位置
        private float xspeed;                 //x轴速度
        private float yspeed;                 //y轴速度
        private float xmaccel;                //x轴加速度
        private float ymaccel;                //y轴加速度
        private float xoldspeed;              //上次更新时x轴速度
        private float yoldspeed;              //上次更新时y轴速度
        private float xoldpos;                //上次更新时x轴位置
        private float yoldpos;                //上次更新时y轴位置
        private int s_id;                     //物体标记
        private float maccel;                 //总加速度
        private DateTime time;                //上次更新时间
        static public int xMaxDistance;       //x轴范围长度
        static public int yMaxDistance;       //y轴范围长度

        public static void set(int xMax, int yMax)
        {
            xMaxDistance = xMax;
            yMaxDistance = yMax;
        }

        public void setID(int ID)
        {
            s_id = ID;
        }

        public int ID()
        {
            return s_id;
        }

        public Cursor2D(float Xpos = 0.0f, float Ypos = 0.0f, float Xspeed = 0.0f, float Yspeed = 0.0f, float xMAccel = 0.0f, float yMAccel = 0.0f)
        {
            xpos = Xpos;
            ypos = Ypos;
            xspeed = Xspeed;
            yspeed = Yspeed;
            xmaccel = xMAccel;
            ymaccel = yMAccel;
            xoldpos = xpos;
            yoldpos = ypos;
            xoldspeed = xspeed;
            yoldspeed = yspeed;
            maccel = (float)Math.Sqrt(xmaccel * xmaccel + ymaccel * ymaccel);
            time = DateTime.Now;
        }

        private void AntiShaking(float time)
        {
        }

        public void update(XYPoint now)    //设定鼠标现在位置为xypoint位置，根据信息更新速度加速度等信息。
        {
            TimeSpan span = DateTime.Now - time;
            float dtime = (float)span.TotalMilliseconds / 1000.0f;
            time = DateTime.Now;
            xoldpos = xpos;
            yoldpos = ypos;
            xoldspeed = xspeed;
            yoldspeed = yspeed;
            xpos = now.x;
            ypos = now.y;
            xspeed = (xpos - xoldpos) / dtime;
            yspeed = (ypos - yoldpos) / dtime;
            xmaccel = (xspeed - xoldspeed) / dtime;
            ymaccel = (yspeed - yoldspeed) / dtime;
            maccel = (float)Math.Sqrt(xmaccel * xmaccel + ymaccel * ymaccel);
            AntiShaking(dtime);
        }

        //返回信息的方法，如public float xPos(bool normalize = false) 
        //Normalize为false，直接返回信息，为true时返回标准化为0-1的信息，如x轴位置信息要除以x轴范围长度xMaxDistance。
        public float xPos(bool normalize = false)     
        {
            if (normalize)
                return xpos / xMaxDistance;
            else
                return xpos;
        }

        public float yPos(bool normalize = false)
        {
            if (normalize)
                return ypos / yMaxDistance;
            else
                return ypos;
        }

        public float xSpeed(bool normalize = false)
        {
            if (normalize)
                return xspeed / xMaxDistance;
            else
                return xspeed;
        }

        public float ySpeed(bool normalize = false)
        {
            if (normalize)
                return yspeed / yMaxDistance;
            else
                return yspeed;
        }

        public float xAccel(bool normalize = false)
        {
            if (normalize)
                return xmaccel / xMaxDistance;
            else
                return xmaccel;
        }

        public float yAccel(bool normalize = false)
        {
            if (normalize)
                return ymaccel / yMaxDistance;
            else
                return ymaccel;
        }

        public float Accel(bool normalize = false)
        {
            if (normalize)
            {
                float x = xmaccel / xMaxDistance;
                float y = ymaccel / yMaxDistance;
                return (float)Math.Sqrt(x * x + y * y);
            }
            else
                return maccel;
        }
    }
}
