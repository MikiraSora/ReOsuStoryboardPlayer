using SimpleRenderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardObject
    {
        public Dictionary<Event, List<Command>> CommandMap = new Dictionary<Event, List<Command>>();

        public string ImageFilePath;

        public int FrameStartTime, FrameEndTime;

        public bool markDone = false;

        public Layout layout;

        public int Z=-1;

        #region Transform

        public Vector Postion=new Vector(320,240), Scale=new Vector(1,1);

        public Vec4 Color=new Vec4(1,1,1,1);

        public float Rotate=0;

        public Vector Anchor=new Vector(0.5f,0.5f);

        #endregion

        public override string ToString() => $"{Z}: {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
    }
}
