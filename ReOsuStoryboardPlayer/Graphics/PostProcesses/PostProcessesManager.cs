using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Graphics.PostProcesses
{
    public class PostProcessesManager
    {
        private SortedList<int, APostProcess> _postProcesses = new SortedList<int, APostProcess>();
        private static APostProcess _finalProcess;
        private int _maxOrder=0;

        private int _currentIndex = 1;
        private PostProcessFrameBuffer[] _fbos = new PostProcessFrameBuffer[2];
        private PostProcessFrameBuffer _prevFbo = null;
        
        public void Init()
        {
            _finalProcess = new FinalPostProcess();
            AddPostProcess(_finalProcess, int.MaxValue);
        }

        public void Resize(int w,int h)
        {
            int sample = 1<<PlayerSetting.SsaaLevel;
            var nw=w*sample;
            var nh=h*sample;

            for (int i = 0; i<_fbos.Length; i++)
            {
                _fbos[i]?.Dispose();
                _fbos[i]=new PostProcessFrameBuffer(nw, nh);
            }

            foreach (var process in _postProcesses)
            {
                process.Value.OnResize();
            }
        }

        public void AddPostProcess(APostProcess postProcess, int order)
        {
            _postProcesses.Add(order, postProcess);
            _maxOrder=Math.Max(_maxOrder, order);
        }

        public void AddPostProcess(APostProcess postProcess)
        {
            if (_postProcesses.ContainsValue(postProcess))
                return;

            _postProcesses.Add(++_maxOrder, postProcess);
        }

        public void RemovePostProcess(APostProcess postProcess)
        {
            var p = _postProcesses.FirstOrDefault(v => v.Value==postProcess);
            if (p.Value!=null)
                _postProcesses.Remove(p.Key);
        }

        public void Begin()
        {
            _prevFbo=_fbos[0];
            _prevFbo.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _currentIndex=1;//reset to 1

            int sample = 1<<PlayerSetting.SsaaLevel;
            GL.Viewport(0, 0, StoryboardWindow.CurrentWindow.Width*sample, StoryboardWindow.CurrentWindow.Height*sample);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Process()
        {
            foreach (var postProcess in _postProcesses)
            {
                var fbo = _fbos[_currentIndex];
                fbo.Bind();

                postProcess.Value.PrevFrameBuffer=_prevFbo;
                postProcess.Value.Process();

                _prevFbo=fbo;
                _currentIndex=(_currentIndex+1)%_fbos.Length;
            }
        }

        public T GetPostProcesser<T>() => _postProcesses.Select(l=>l.Value).OfType<T>().FirstOrDefault();
    }
}