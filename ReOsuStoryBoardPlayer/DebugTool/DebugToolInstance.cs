using OpenTK;
using ReOsuStoryBoardPlayer.DebugTool.ObjectInfoVisualizer;
using System;
using System.Collections.Generic;
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
            InitStoryboardObjectVisualizerWindow();
        }

        public void Update()
        {
            CallUpdateDebugControllerWindowInfo();
            UpdateVisualizerWindow();
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
            VisualizerWindow = new ObjectInfoVisualizerWindow();
            VisualizerWindow.Show();
        }

        private void UpdateVisualizerWindow() => VisualizerWindow.UpdateCurrentStoryboardObject();

        public void SelectObjectIntoVisualizer(float x, float y)
        {
            int last_order_index = VisualizerWindow.obj == null ? -1 : VisualizerWindow.obj.Z;

            var obj = (from temp in refInstance.StoryboardObjectList where temp.Z > last_order_index && IsPointInObjectArea(temp, x, y) select temp).FirstOrDefault();

            VisualizerWindow.obj = obj;
        }

        Vector3 _staticCacheAxis = new Vector3(0, 0, 1);

        bool IsPointInObjectArea(StoryBoardObject sb_obj, float x, float y)
        {
            //ViewProjection*in_model*vec4((in_pos-in_anchor)*in_bound,in_Z,1.0);
            var mouse_point = new Vector2(x, y);

            float[] _cacheBaseVertex = new float[] {
                0,0,
                0,-1,
                1,-1,
                1,0,
            };
            Vector3[] points = new Vector3[4];

            Vector2 in_anchor = new Vector2(sb_obj.Anchor.x, -sb_obj.Anchor.y);

            float w = refInstance.CacheDrawSpriteInstanceMap[sb_obj.ImageFilePath].Texture.Width ;
            float h = refInstance.CacheDrawSpriteInstanceMap[sb_obj.ImageFilePath].Texture.Height ;

            Vector2 in_bound = new Vector2(w, h);

            Matrix4 in_model =
                Matrix4.Identity *
            Matrix4.CreateScale(sb_obj.Scale.x, sb_obj.Scale.y, 1) *
            Matrix4.CreateFromAxisAngle(_staticCacheAxis, sb_obj.Rotate / 180.0f * 3.1415926f) *
            Matrix4.CreateTranslation(sb_obj.Postion.x - StoryboardWindow.CurrentWindow.Width / 2, -sb_obj.Postion.y + StoryboardWindow.CurrentWindow.Height / 2, 0);
            
            for (int i = 0; i < 4; i++)
            {
                var vertex = new Vector2(_cacheBaseVertex[i * 2 + 0], _cacheBaseVertex[i * 2 + 1]);
                var temp = (vertex - in_anchor) * in_bound;
                var transform = StoryboardWindow.CameraViewMatrix*in_model * new Vector4(temp.X,temp.Y,0,1);
                points[i] = new Vector3(mouse_point-new Vector2(transform.X, transform.Y));
            }

            Vector3 
                result_vec1 = Vector3.Cross(points[0], points[1]),
                result_vec2= Vector3.Cross(points[0], points[2]) , 
                result_vec3= Vector3.Cross(points[0], points[3]) , 
                result_vec4= Vector3.Cross(points[1], points[2]) , 
                result_vec5= Vector3.Cross(points[1], points[3]) , 
                result_vec6= Vector3.Cross(points[2], points[1]);

            return true;
        }

        public void CannelSelectObject()
        {
            VisualizerWindow.obj = null;
        }

    }
}
