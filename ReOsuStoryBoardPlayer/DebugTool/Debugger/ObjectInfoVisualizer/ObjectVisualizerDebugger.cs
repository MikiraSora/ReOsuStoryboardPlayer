using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer
{
    class ObjectVisualizerDebugger : DebuggerBase
    {
        ObjectVisualizerWindow window;

        public ObjectVisualizerDebugger()
        {
            Priority=UpdatePriority.EveryFrame;
        }

        public override void Init()
        {
            window=new ObjectVisualizerWindow(StoryBoardInstance.Instance);
            window.Show();

            DebuggerManager.MouseClick+=DebuggerManager_MouseClick;
        }

        private void DebuggerManager_MouseClick(int x, int y, MouseInput input)
        {
            switch (input)
            {
                case MouseInput.Right:
                    window.obj=null;
                    break;
                case MouseInput.Left:
                    PickObject(x, y);
                    break;
                default:
                    break;
            }
        }

        #region Pick object

        private void PickObject(float x,float y)
        {
            int last_order_index = window?.obj?.Z??-1;

            StoryBoardObject obj = null;

            foreach (var list in StoryBoardInstance.Instance._UpdatingStoryBoard.Values)
            {
                foreach (var temp in list)
                {
                    if (temp.Z>last_order_index&&
                        (obj==null ? true : (temp.Z<obj.Z))&&
                        IsPointInObjectArea(temp, x, y))
                    {
                        obj=temp;
                    }
                }
            }

            window.obj=obj;
        }

        private static readonly Vector3 _staticCacheAxis = new Vector3(0, 0, 1);

        private static readonly float[] _cacheBaseVertex = new float[] {
                0,0,
                0,-1,
                1,-1,
                1,0,
            };

        private bool IsPointInObjectArea(StoryBoardObject sb_obj, float x, float y)
        {
            var mouse_point = new Vector2(x, y);
            Vector3[] points = new Vector3[4];

            Vector2 in_anchor = new Vector2(sb_obj.Anchor.x, -sb_obj.Anchor.y);

            float w = sb_obj.RenderGroup.Texture.Width;
            float h = sb_obj.RenderGroup.Texture.Height;

            Vector2 in_bound = new Vector2(w, h);

            //将物件的坐标投影到当前屏幕大小
            var fix_obj_pos = new Vector(
                sb_obj.Postion.x/StoryboardWindow.SB_WIDTH*StoryboardWindow.CurrentWindow.Width,
                sb_obj.Postion.y/StoryboardWindow.SB_HEIGHT*StoryboardWindow.CurrentWindow.Height
                );

            //将物件的缩放投影到当前屏幕大小
            var fix_obj_size = new Vector(
                sb_obj.Scale.x/StoryboardWindow.SB_WIDTH*StoryboardWindow.CurrentWindow.Width,
                sb_obj.Scale.y/StoryboardWindow.SB_HEIGHT*StoryboardWindow.CurrentWindow.Height
                );

            Matrix4 in_model =
                Matrix4.Identity*
            Matrix4.CreateScale(fix_obj_size.x, fix_obj_size.y, 1)*
            Matrix4.CreateFromAxisAngle(_staticCacheAxis, sb_obj.Rotate/180.0f*3.1415926f)*
            Matrix4.CreateTranslation(fix_obj_pos.x-StoryboardWindow.CurrentWindow.Width/2, -fix_obj_pos.y+StoryboardWindow.CurrentWindow.Height/2, 0);

            mouse_point.X=mouse_point.X-StoryboardWindow.CurrentWindow.Width/2;
            mouse_point.Y=StoryboardWindow.CurrentWindow.Height/2-mouse_point.Y;

            for (int i = 0; i<4; i++)
            {
                var vertex = new Vector2(_cacheBaseVertex[i*2+0], _cacheBaseVertex[i*2+1]);
                var temp = (vertex-in_anchor)*in_bound;
                var transform = new Vector4(temp.X, temp.Y, 0, 1)*StoryboardWindow.CameraViewMatrix*in_model;

                points[i]=new Vector3(mouse_point-new Vector2(transform.X, transform.Y));
            }

            Vector3 v1 = Vector3.Cross(points[0], points[1]).Normalized();
            Vector3 v2 = Vector3.Cross(points[1], points[2]).Normalized();
            Vector3 v3 = Vector3.Cross(points[2], points[3]).Normalized();
            Vector3 v4 = Vector3.Cross(points[3], points[0]).Normalized();

            if (Vector3.Dot(v1, v2)>0.99999f&&Vector3.Dot(v2, v3)>0.9999f&&
                Vector3.Dot(v3, v4)>0.9999f&&Vector3.Dot(v4, v1)>0.9999f)
                return true;
            return false;
        }

        #endregion

        public override void Term()
        {
            window.Close();
            DebuggerManager.MouseClick-=DebuggerManager_MouseClick;
        }

        public override void Update()
        {
            window.UpdateCurrentStoryboardObject();
        }
    }
}
