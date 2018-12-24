using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using ReOsuStoryBoardPlayer.Graphics.PostProcesses.Shaders;

namespace ReOsuStoryBoardPlayer.Graphics.PostProcesses
{
    public class PostProcessesManager
    {
        private SortedList<int,APostProcess> _postProcesses = new SortedList<int,APostProcess>();
        private APostProcess _finalProcess= new FinalPostProcess();
        private int _maxOrder = 0;

        private int _currentIndex = 1;
        private PostProcessFrameBuffer[] _fbos = new PostProcessFrameBuffer[2];
        private PostProcessFrameBuffer _lastFbo = null;

        public PostProcessesManager(int w,int h)
        {
            for (int i = 0; i < _fbos.Length; i++)
            {
                _fbos[i] = new PostProcessFrameBuffer(w,h);
            }
        }

        public void AddPostProcess(APostProcess postProcess, int order)
        {
            _postProcesses.Add(order, postProcess);
            _maxOrder = Math.Max(_maxOrder, order);
        }

        public void AddPostProcess(APostProcess postProcess)
        {
            _postProcesses.Add(++_maxOrder, postProcess);
        }

        public void RemovePostProcess(APostProcess postProcess)
        {
            var p = _postProcesses.First(v => v.Value == postProcess);
            _postProcesses.Remove(p.Key);
        }

        public void Begin()
        {
            _fbos[0].Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _lastFbo = _fbos[0];
            _currentIndex = 1;
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

                postProcess.Value.LastFrameBuffer = _lastFbo;
                postProcess.Value.Process();

                _lastFbo = fbo;
                _currentIndex = (_currentIndex + 1) % _fbos.Length;
            }

            _finalProcess.LastFrameBuffer = _lastFbo;
            _finalProcess.Process();
        }
    }
}
