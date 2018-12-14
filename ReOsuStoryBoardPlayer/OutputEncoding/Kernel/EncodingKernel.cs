using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.OutputEncoding.Player;
using ReOsuStoryBoardPlayer.Player;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding.Kernel
{
    /// <summary>
    /// 编码功能的核心，负责管控视频编码
    /// </summary>
    public class EncodingKernel:DebuggerBase
    {
        EncodingProcessPlayer time_control;
        //Thread thread;
        EncodingWriterBase writer;
        EncoderOption option;

        float prev_time;

        bool is_running = false;

        byte[] buffer;

        public EncodingKernel(EncoderOption option)
        {
            this.option=option;
        }

        public void Start()
        {
            time_control=MusicPlayerManager.ActivityPlayer as EncodingProcessPlayer;

            if (time_control==null)
                throw new Exception("Current player isn't EncodingProcessPlayer!");

            writer=EncodingWriterFatory.Create();
            writer.OnStart(option);

            Log.User($"Start encoding....");

            /*
            thread=new Thread(Run);
            thread.Start();
            */

            buffer=new byte[option.Width*option.Height*4];
            is_running=true;

            DebuggerManager.AfterRender+=OnAfterRender;
            prev_time=time_control.CurrentTime;
        }

        public void OnAfterRender()
        {
            time_control.GetNextFrameTime();

            if ((!time_control.IsPlaying)&&is_running)
            {
                //过时，关闭
                writer.OnFinish();
                is_running=false;
                return;
            }

            if (!CheckCondition())
                return;

            Log.Debug($"Process time : {time_control.CurrentTime} ({(time_control.CurrentTime/time_control.Length*100).ToString("F2")})");
       
            GL.ReadPixels(0, 0, option.Width, option.Height, PixelFormat.Bgra, PixelType.UnsignedByte, ref buffer[0]);

            for (int i = 0; i<option.Height/2; i++)
            {
                var l = option.Height-i-1;

                var src = i*option.Width*4;
                var dist = l*option.Width*4;

                for (int x = 0; x<option.Width*4; x++)
                {
                    var z = buffer[src+x];
                    buffer[src+x]=buffer[dist+x];
                    buffer[dist+x]=z;
                }
            }

            writer.OnNextFrame(buffer, option.Width, option.Height);
        }

        private bool CheckCondition()
        {
            return time_control.IsPlaying&&prev_time<time_control.CurrentTime;
        }

        public void Abort()
        {
            if (is_running)
            {
                writer.OnAbort();
                Log.User($"Encoding abort");
            }
        }

        public override void Init()
        {

        }

        public override void Term()
        {

        }

        public override void Update()
        {

        }
    }
}
