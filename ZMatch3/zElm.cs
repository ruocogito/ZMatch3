using System;
using System.Collections.Generic;
using System.Text;
/*
 Game elent logic class
 */

namespace ZMatch3
{
    class zElm
    {
        public bool isanimated { get; set; }
        private zAnimation anm;
        public (float, float) getTmtrxXY((float, float) existedTxy) => isanimated ? anm.getTmtrxXY(existedTxy) : existedTxy;
        /*
         orgij - Animation Start Point (this is not element coordinates)
         limit - Count of ticks for animate
         delta - velocity: NDC per tick
         xanim - true if move on horizontal
         isfakeexch - true for user choisen elm (for back it if no match)
         * */
        public void StartAnimation((ushort, ushort) orgij,int limit, float delta, bool xanim, bool isfakeexch=false)
        {
            anm = new zAnimation(orgij, limit,  delta, xanim);
            isanimated = true;
            isfakeexchanged = isfakeexch;
        }
        public bool IsAnmFinished() => isanimated ? anm.IsFinished() : false;
        public (ushort, ushort) CalcNeighbor(ushort i, ushort j) => anm.CalcNeighbor((short)i, (short)j);

        public (ushort, ushort) getorgij { get { return anm.getorgij; } }
        public float rotangle { get; set; }
        public ushort elmtype { get; set; }
        //True for just moved mat be not muched elm
        public bool isfakeexchanged { get; set; }
        /*
         Destroy function set this flag and gravity function
         * */
        public bool IsDestroed { get; set; }
        /*
         This flags seting before elms destoed
         When Gravity func see this flags, it spawn bomb bonus and etc.
         * */
        public bool SpawnBombThere { get; set; }
        public bool SpawnHReaperThere { get; set; }
        public bool SpawnVReaperThere { get; set; }
        //this is bonus
        public bool IsBomb { get; set; }
        //This is not a bonus, flag to spawn it, if 4 match
        public ushort ReaperType { get; set; }
        //delay in ticks before new top elms will be draw
        public int waittospawn { get; set; }
        public zElm(ushort type=0,ushort reapertype=0)
       {
            isanimated = isfakeexchanged = false;
            elmtype = type;
            rotangle = 0; 
            IsDestroed = false;
            IsBomb = false;
            ReaperType = reapertype;
            waittospawn = 0;
            SpawnBombThere = false;
       }
        public void Think(float drot)
        {
            rotangle += drot;


            if (isanimated) anm.Think();
        }
      

    }

    class zAnimation
    {
        private float tickcounter { get; set; }
        private int ticklimit;
        private float tickdelta;
        private bool isXanimation;
        (ushort, ushort) orgij;
        public (ushort, ushort) getorgij { get { return orgij; }  }
        public (float,float) getTmtrxXY((float,float) existedTxy)
        {
            float tx = isXanimation ? tickcounter * tickdelta : 0;
            float ty = !isXanimation ? tickcounter * tickdelta : 0;

            return (existedTxy.Item1+tx,existedTxy.Item2+ty);
        }
        public (ushort, ushort) CalcNeighbor(short i, short j)
        {
            if (isXanimation)
                i += tickdelta > 0 ? 1 : -1;
            else
                j += tickdelta > 0 ? -1 : 1;
            return ((ushort)i, (ushort)j);
        }
        public void Think() => tickcounter += 1f;
        public bool IsFinished() => tickcounter == ticklimit;
        public zAnimation((ushort, ushort) orgij, int limit, float delta, bool xanim)
        {
            ticklimit = limit;
            tickdelta = delta;
            isXanimation = xanim;
            tickcounter = 0;
            this.orgij = orgij;
        }
    }
}
