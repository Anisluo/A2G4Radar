using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    class SIDManagement
    {
        private Stack<int> IDunUse = new Stack<int>();
        private int NowMaxID = 0;
        public void clear()
        {
            IDunUse.Clear();
            NowMaxID = 0;
        }

        public void freeID(int s_id)     //释放一个当前被使用的s_ID
        {
            IDunUse.Push(s_id);
        }

        public int getID()               //获得一个未被使用的s_ID
        {
            if (IDunUse.Count != 0)
                return IDunUse.Pop();
            else
            {
                NowMaxID++;
                return NowMaxID;
            }
        }
    }
}
