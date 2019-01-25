using OpenTK.Graphics.OpenGL;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.OutputEncoding.Player;
using ReOsuStoryboardPlayer.Player;
using System;

namespace ReOsuStoryboardPlayer.OutputEncoding.Kernel
{
    /// <summary>
    /// 编码功能的核心，负责管控视频编码
    /// </summary>
    public class EncodingKernel : ToolBase
    {
        private EncodingProcessPlayer time_control;

        //Thread thread;
        public EncodingWriterBase Writer { get; private set; }

        private EncoderOption option;

        private float prev_time;

        private bool is_running = false;

        private byte[] buffer;

        public EncodingKernel(EncoderOption option)
        {
            this.option=option;
        }

        private void OnKeyPress(OpenTK.Input.Key obj)
        {
            if (obj==OpenTK.Input.Key.E)
                Abort();
        }

        public void Start()
        {
            time_control=MusicPlayerManager.ActivityPlayer as EncodingProcessPlayer;

            if (time_control==null)
                throw new Exception("Current player isn't EncodingProcessPlayer!");

            Writer=EncodingWriterFatory.Create();
            Writer.OnStart(option);

            Log.User($"Start encoding....");

            buffer=new byte[option.Width*option.Height*4];
            is_running=true;

            if (option.IsExplicitTimeRange)
            {
                time_control.Jump(option.StartTime, true);
            }

            ToolManager.AfterRender+=OnAfterRender;
            prev_time=time_control.CurrentTime;
        }

        public void OnAfterRender()
        {
            time_control.GetNextFrameTime();

            //超出时间，结束
            if (time_control.CurrentTime>=(option.IsExplicitTimeRange ? Math.Min(time_control.Length, option.EndTime) : time_control.Length))
            {
                Abort();
                return;
            }

            //时间轴不变，跳过
            if (prev_time>=time_control.CurrentTime)
                return;

            prev_time=time_control.CurrentTime;

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

            Writer.OnNextFrame(buffer, option.Width, option.Height);
        }

        public void Abort()
        {
            if (is_running)
            {
                Writer.OnFinish();
                is_running=false;
                ToolManager.AfterRender-=OnAfterRender;
                Log.User($"Encoding abort");
            }
        }

        public override void Init()
        {
            ToolManager.KeyboardPress+=OnKeyPress;
        }

        public override void Term()
        {
            ToolManager.KeyboardPress-=OnKeyPress;
            Abort();
        }

        public override void Update()
        {
        }
    }
}