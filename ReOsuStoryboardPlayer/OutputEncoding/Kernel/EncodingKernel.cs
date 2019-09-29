using OpenTK.Graphics.OpenGL;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.OutputEncoding.Player;
using ReOsuStoryboardPlayer.Player;
using System;
using ReOsuStoryBoardPlayer.OutputEncoding.Graphics.PostProcess;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using ReOsuStoryBoardPlayer.Graphics;

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

        public EncoderOption Option { get; private set; }

        CaptureRenderPostProcess capturer;

        private float prev_time;

        private bool is_running = false;

        public EncodingKernel(EncoderOption option)
        {
            this.Option=option;
        }

        private void OnKeyPress(OpenTK.Input.Key obj)
        {
            if (obj==OpenTK.Input.Key.E)
                Abort();
        }

        public void Start()
        {
            time_control=MusicPlayerManager.ActivityPlayer as EncodingProcessPlayer;

            if (capturer==null)
            {
                //init capturer post process
                capturer=new CaptureRenderPostProcess();
                RenderKernel.PostProcessesManager.AddPostProcess(capturer);
            }

            if (time_control==null)
                throw new Exception("Current player isn't EncodingProcessPlayer!");

            Writer=EncodingWriterFatory.Create();
            Writer.OnStart(Option);

            Log.User($"Start encoding....");

            is_running=true;

            if (Option.IsExplicitTimeRange)
            {
                time_control.Jump(Option.StartTime, true);
            }

            ToolManager.AfterRender+=OnAfterRender;
            prev_time=time_control.CurrentTime;
        }

        public void OnAfterRender()
        {
            time_control.GetNextFrameTime();

            //超出时间，结束
            if (time_control.CurrentTime>=(Option.IsExplicitTimeRange ? Math.Min(time_control.Length, Option.EndTime) : time_control.Length))
            {
                Abort();
                return;
            }

            //时间轴不变，跳过
            if (prev_time>=time_control.CurrentTime)
                return;

            prev_time=time_control.CurrentTime;

            Log.Debug($"Process time : {time_control.CurrentTime} ({(time_control.CurrentTime/time_control.Length*100).ToString("F2")})");
            
            Writer.OnNextFrame(capturer.RenderPixels, Option.Width, Option.Height);
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