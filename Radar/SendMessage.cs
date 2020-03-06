using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    class SingleMessage              //SingleMessage使用方法：调用set/alive/fseqCursor2DMessage后
    {                                //便可以直接从public byte[] message中获取对应的消息
        private static byte[] cur2D = Encoding.UTF8.GetBytes("/tuio/2Dcur\0");
        private static byte[] set = Encoding.UTF8.GetBytes("set\0");
        private static byte[] alive = Encoding.UTF8.GetBytes("alive\0\0\0");
        private static byte[] fseq = Encoding.UTF8.GetBytes("fseq\0\0\0\0");
        private bool if_set = false;
        public byte[] message;

        public bool IFset()
        {
            return if_set;
        }

        public static byte[] swapEndian(byte[] data)    //将data里的数据置换顺序（用于调整大小端）
        {
            byte[] swapped = new byte[data.Length];
            for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
            {
                swapped[j] = data[i];
            }
            return swapped;
        }

        public static byte[] packInt(int value)         //将int类型的值转化为byte数组
        {
            byte[] data = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) data = swapEndian(data);
            return data;
        }

        protected static byte[] packFloat(float value)  //将float类型的值转化为byte数组
        {
            byte[] data = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) data = swapEndian(data);
            return data;
        }

        public byte[] setCur2DMessage(Cursor2D x)       //根据cursor2D的值设置message为set类型的消息
        {
            return setCur2DMessage(x.ID(), x.xPos(true), x.yPos(true), x.xSpeed(true), x.ySpeed(true), x.Accel(true));
        }

        public byte[] setCur2DMessage(int s_id, float xPos, float yPos, float xSpeed, float ySpeed, float mAccele)
        {
            byte[] intMessage = packInt(s_id);
            byte[] floatMessage = packFloat(xPos);
            byte[] tagMessage = Encoding.UTF8.GetBytes("sifffff\0");
            int length = cur2D.Length + tagMessage.Length + set.Length + intMessage.Length + floatMessage.Length * 5;
            message = new byte[length];
            int start = 0;
            cur2D.CopyTo(message, start);
            start += cur2D.Length;
            tagMessage.CopyTo(message, start);
            start += tagMessage.Length;
            set.CopyTo(message, start);
            start += set.Length;
            intMessage.CopyTo(message, start);
            start += intMessage.Length;
            floatMessage.CopyTo(message, start);
            start += floatMessage.Length;
            floatMessage = packFloat(yPos);
            floatMessage.CopyTo(message, start);
            start += floatMessage.Length;
            floatMessage = packFloat(xSpeed);
            floatMessage.CopyTo(message, start);
            start += floatMessage.Length;
            floatMessage = packFloat(ySpeed);
            floatMessage.CopyTo(message, start);
            start += floatMessage.Length;
            floatMessage = packFloat(mAccele);
            floatMessage.CopyTo(message, start);
            start += floatMessage.Length;
            if_set = true;
            return message;
        }

        public byte[] fseqCur2DMessage(int f_id)            //根据f_ID设置message为对应的fseq消息类型
        {
            byte[] intMessage = packInt(f_id);
            byte[] tagMessage = Encoding.UTF8.GetBytes("si\0\0");
            int length = cur2D.Length + tagMessage.Length + fseq.Length + intMessage.Length;
            message = new byte[length];
            int start = 0;
            cur2D.CopyTo(message, start);
            start += cur2D.Length;
            tagMessage.CopyTo(message, start);
            start += tagMessage.Length;
            fseq.CopyTo(message, start);
            start += fseq.Length;
            intMessage.CopyTo(message, start);
            start += intMessage.Length;
            if_set = true;
            return message;
        }

        public byte[] aliveCur2DMessage(ref int[] s_ids)    //根据s_IDs里的数据设置message为alive类型的消息
        {
            int i;
            string tag = "s";
            for (i = 0; i < s_ids.Length; i++)
                tag = tag + "i";
            tag = tag + "\0";
            int j = (tag.Length + 3) / 4 * 4;
            for (i = tag.Length; i < j; i++)
                tag = tag + "\0";
            byte[] tagMessage = Encoding.UTF8.GetBytes(tag);
            byte[] intMessage = packInt(1);
            int length = cur2D.Length + tagMessage.Length + alive.Length + s_ids.Length * intMessage.Length;
            int start = 0;
            message = new byte[length];
            cur2D.CopyTo(message, start);
            start += cur2D.Length;
            tagMessage.CopyTo(message, start);
            start += tagMessage.Length;
            alive.CopyTo(message, start);
            start += alive.Length;
            for (i = 0; i < s_ids.Length; i++)
            {
                intMessage = packInt(s_ids[i]);
                intMessage.CopyTo(message, start);
                start += intMessage.Length;
            }
            if_set = true;
            return message;
        }
    }

    class MultiMessage
    {
        public static readonly DateTime Epoch = new DateTime(1900, 1, 1, 0, 0, 0, 0);

        public static bool IsValidTime(DateTime timeStamp)
        {
            return (timeStamp >= Epoch + TimeSpan.FromMilliseconds(1.0));
        }

        public static byte[] TimeToByteArray(DateTime mTimeStamp)
        {
            List<byte> timeStamp = new List<byte>();

            byte[] secondsSinceEpoch = BitConverter.GetBytes((uint)(mTimeStamp - Epoch).TotalSeconds);
            byte[] fractionalSecond = BitConverter.GetBytes((uint)((mTimeStamp - Epoch).Milliseconds));

            if (BitConverter.IsLittleEndian) // != OscPacket.LittleEndianByteOrder)
            {
                secondsSinceEpoch = SingleMessage.swapEndian(secondsSinceEpoch);
                fractionalSecond = SingleMessage.swapEndian(fractionalSecond);
            }

            timeStamp.AddRange(secondsSinceEpoch);
            timeStamp.AddRange(fractionalSecond);

            return timeStamp.ToArray();
        }

        protected static byte[] packTimeTag(DateTime value)          //将时间信息打包成byte数组
        {
            value = new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerMillisecond), value.Kind);
            if (!IsValidTime(value)) throw new Exception("Not a valid OSC Timetag.");
            return TimeToByteArray(value);
        }

        public byte[] message;
        private static byte[] bundle = Encoding.UTF8.GetBytes("#bundle\0");
        private List<SingleMessage> components = new List<SingleMessage>();
        private int length = 0;
        private DateTime time = new DateTime(2000, 1, 1, 0, 0, 0, 0);
        private byte[] timeMessage;
        private const int intlength = 4;

        public bool setTime(DateTime x)
        {
            if (IsValidTime(x))
                return false;
            else
                time = x;
            return true;
        }

        public void clear()                                 //将multiMessage存储的信息清空
        {
            components.Clear();
            length = 0;
        }

        public bool add(SingleMessage x)                    //添加singleMessage x所包含的信息到包里
        {
            if (x.IFset())
            {
                SingleMessage y = new SingleMessage();
                y.message = new byte[x.message.Length];
                x.message.CopyTo(y.message, 0);
                components.Add(y);
                length = y.message.Length + intlength + length;
                return true;
            }
            else
                return false;
        }
        //Add方法添加的singleMessage均通过深度复制添加到private List<SingleMessage> components里
        //待最后getMessage时一起转化为byte数组信息。

        public byte[] getMessage()                          //获得目前为止存储的总信息
        {
            time = DateTime.Now;
            timeMessage = packTimeTag(time);
            length = length + bundle.Length + timeMessage.Length;
            message = new byte[length];
            byte[] intMessage;
            int start = 0;
            bundle.CopyTo(message, start);
            start += bundle.Length;
            timeMessage.CopyTo(message, start);
            start += timeMessage.Length;
            foreach (SingleMessage x in components)
            {
                intMessage = SingleMessage.packInt(x.message.Length);
                intMessage.CopyTo(message, start);
                start += intlength;
                x.message.CopyTo(message, start);
                start += x.message.Length;
            }
            return message;
        }

        //MultiMessage使用方法：clear()->singleMessage.set/alive/fseqCur2DMessage->add(x)->getMessage->clear();
        //MultiMessage使用建议：每次生成multimessage时，建议一次只添加set、alive、fseq消息各一个
    }
}
