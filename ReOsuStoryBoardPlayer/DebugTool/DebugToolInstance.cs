using OpenTK;
using ReOsuStoryBoardPlayer.DebugTool.ObjectInfoVisualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class DebugToolInstance
    {
        StoryBoardInstance refInstance;

        DebugController.ControllerWindow ControllerWindow;

        ObjectInfoVisualizerWindow VisualizerWindow;

        string debug_break_storyboard_image;

        Event debug_break_event;

        public DebugToolInstance(StoryBoardInstance instance)
        {
            refInstance = instance;
            InitDebugControllerWindow();

#if DEBUG
            InitStoryboardObjectVisualizerWindow();
#endif
        }

        public void Update()
        {
            CallUpdateDebugControllerWindowInfo();

#if DEBUG
            UpdateVisualizerWindow();
#endif
        }

        public void DumpCurrentStoryboardStatus()
        {
            foreach (var layout in refInstance._UpdatingStoryBoard)
            {
                Log.User($"Dump Layout:{layout.Key.ToString()}");
                foreach (var obj in layout.Value)
                {
                    Log.User($"\"{obj.ImageFilePath}\" \\ Z = {obj.Z} \\ {obj.FrameStartTime} ~ {obj.FrameEndTime} \nPosition={obj.Postion} \\ Rotate = {obj.Rotate} \\ Scale = {obj.Scale} \n Color = {obj.Color} \\ Anchor : {obj.Anchor} \n A:{obj.IsAdditive} / H:{obj.IsHorizonFlip} / V:{obj.IsVerticalFlip} \n-----------------------");
                }
            }
        }

        public void CreateBreakpointInCommandExecuting(string break_storyboard_image, Event break_event)
        {
            this.debug_break_event = break_event;
            this.debug_break_storyboard_image = break_storyboard_image.Trim().Replace("/", "\\");
            refInstance.Flush();
        }

        public void ClearBreakpoint()
        {
            this.debug_break_storyboard_image = string.Empty;
        }

        public void CallUpdateDebugControllerWindowInfo() => ControllerWindow.UpdateInfo();

        public void InitDebugControllerWindow()
        {
            ControllerWindow = new DebugController.ControllerWindow(refInstance);
            ControllerWindow.Show();
            ControllerWindow.progressBar1.Maximum = (int)refInstance.player.Length;
        }

        public void InitStoryboardObjectVisualizerWindow()
        {
            VisualizerWindow = new ObjectInfoVisualizerWindow(refInstance);
            VisualizerWindow.Show();
        }

        private void UpdateVisualizerWindow() => VisualizerWindow.UpdateCurrentStoryboardObject();

        public void SelectObjectIntoVisualizer(float x, float y)
        {
            int last_order_index = VisualizerWindow?.obj?.Z??-1;

            StoryBoardObject obj=null;

            foreach (var list in refInstance._UpdatingStoryBoard.Values)
            {
                foreach (var temp in list)
                {
                    if (temp.Z > last_order_index &&
                        (obj == null ? true : (temp.Z < obj.Z)) &&
                        IsPointInObjectArea(temp, x, y))
                    {
                        obj = temp;
                    }
                }
            }

            VisualizerWindow.obj = obj;
        }

        static readonly Vector3 _staticCacheAxis = new Vector3(0, 0, 1);

        static readonly float[] _cacheBaseVertex = new float[] {
                0,0,
                0,-1,
                1,-1,
                1,0,
            };

        bool IsPointInObjectArea(StoryBoardObject sb_obj, float x, float y)
        {
            var mouse_point = new Vector2(x, y);
            Vector3[] points = new Vector3[4];

            Vector2 in_anchor = new Vector2(sb_obj.Anchor.x, -sb_obj.Anchor.y);

            float w = sb_obj.RenderGroup.Texture.Width ;
            float h = sb_obj.RenderGroup.Texture.Height ;

            Vector2 in_bound = new Vector2(w, h);

            //将物件的坐标投影到当前屏幕大小
            var fix_obj_pos = new Vector(
                sb_obj.Postion.x / 640 * StoryboardWindow.CurrentWindow.Width,
                sb_obj.Postion.y / 480 * StoryboardWindow.CurrentWindow.Height
                );

            //将物件的缩放投影到当前屏幕大小
            var fix_obj_size = new Vector(
                sb_obj.Scale.x / 640 * StoryboardWindow.CurrentWindow.Width,
                sb_obj.Scale.y / 480 * StoryboardWindow.CurrentWindow.Height
                );

            Matrix4 in_model =
                Matrix4.Identity *
            Matrix4.CreateScale(fix_obj_size.x, fix_obj_size.y, 1) *
            Matrix4.CreateFromAxisAngle(_staticCacheAxis, sb_obj.Rotate / 180.0f * 3.1415926f) *
            Matrix4.CreateTranslation(fix_obj_pos.x - StoryboardWindow.CurrentWindow.Width / 2, -fix_obj_pos.y + StoryboardWindow.CurrentWindow.Height / 2, 0);

            mouse_point.X = mouse_point.X - StoryboardWindow.CurrentWindow.Width / 2;
            mouse_point.Y = StoryboardWindow.CurrentWindow.Height / 2 - mouse_point.Y;
            
            for (int i = 0; i < 4; i++)
            {
                var vertex = new Vector2(_cacheBaseVertex[i * 2 + 0], _cacheBaseVertex[i * 2 + 1]);
                var temp = (vertex - in_anchor) * in_bound;
                var transform = new Vector4(temp.X, temp.Y, 0, 1) * StoryboardWindow.CameraViewMatrix * in_model;

                points[i] = new Vector3(mouse_point-new Vector2(transform.X, transform.Y) );
            }

            Vector3 v1 = Vector3.Cross(points[0], points[1]).Normalized();
            Vector3 v2 = Vector3.Cross(points[1], points[2]).Normalized();
            Vector3 v3 = Vector3.Cross(points[2], points[3]).Normalized();
            Vector3 v4 = Vector3.Cross(points[3], points[0]).Normalized();

            if (Vector3.Dot(v1, v2) > 0.99999f && Vector3.Dot(v2, v3) > 0.9999f &&
                Vector3.Dot(v3, v4) > 0.9999f && Vector3.Dot(v4, v1) > 0.9999f)
                return true;
            return false;

        }

        public void CannelSelectObject()
        {
            VisualizerWindow.obj = null;
        }

    }
}
