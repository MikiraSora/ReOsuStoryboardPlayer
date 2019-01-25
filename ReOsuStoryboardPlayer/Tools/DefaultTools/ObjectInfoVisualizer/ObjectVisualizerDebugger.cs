using OpenTK;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Kernel;
using System;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectInfoVisualizer
{
    internal class ObjectVisualizerDebugger : ToolBase
    {
        public ObjectVisualizerWindow Window { get; private set; }

        public ObjectVisualizerDebugger()
        {
            Priority=UpdatePriority.EveryFrame;
        }

        public override void Init()
        {
            Window=new ObjectVisualizerWindow(StoryboardInstanceManager.ActivityInstance);
            Window.Show();

            ToolManager.MouseClick+=OnMouseClick;
            ToolManager.BeforeRender+=OnBeforeRender;
            ToolManager.AfterRender+=OnAfterRender;
        }

        private void OnAfterRender()
        {
            var select_object = Window.SelectObject;

            if (select_object==null)
                return;

            select_object.Color.W=backup_alpha;
        }

        private byte backup_alpha;
        private float count;

        private void OnBeforeRender()
        {
            var select_object = Window.SelectObject;

            if (select_object==null)
            {
                count=0;
                return;
            }

            count+=0.00045f*DateTime.Now.Second;

            backup_alpha=select_object.Color.W;
            select_object.Color.W=(byte)Math.Min(255, Math.Abs(255*Math.Cos(count)));
        }

        private void OnMouseClick(int x, int y, MouseInput input)
        {
            switch (input)
            {
                case MouseInput.Right:
                    Window.SelectObject=null;
                    break;

                case MouseInput.Left:
                    PickObject(x, y);
                    break;

                default:
                    break;
            }
        }

        #region Pick object

        private void PickObject(float x, float y)
        {
            int last_order_index = Window?.SelectObject?.Z??-1;

            StoryboardObject obj = null;

            foreach (var temp in StoryboardInstanceManager.ActivityInstance.Updater.UpdatingStoryboardObjects)
            {
                if (temp.Z>last_order_index&&
                    (obj==null ? true : (temp.Z<obj.Z))&&
                    IsPointInObjectArea(temp, x, y))
                {
                    obj=temp;
                }
            }

            Window.SelectObject=obj;
        }

        private const float DEG2RAD = 0.017453292519943295f;

        private bool IsPointInObjectArea(StoryboardObject obj, float x, float y)
        {
            Vector2 obj_pos = new Vector2(obj.Postion.X, obj.Postion.Y);

            float radio = (float)StoryboardWindow.CurrentWindow.Width/(float)StoryboardWindow.CurrentWindow.Height;
            float view_width = StoryboardWindow.SB_HEIGHT*radio;

            Vector2 mouse_scale = new Vector2(view_width/StoryboardWindow.CurrentWindow.Width,
                StoryboardWindow.SB_HEIGHT/StoryboardWindow.CurrentWindow.Height);

            var mouse_point = new Vector2(x, y)*mouse_scale;

            mouse_point.X-=(view_width-StoryboardWindow.SB_WIDTH)/2.0f;

            Vector3[] points = new Vector3[4];

            var group = StoryboardInstanceManager.ActivityInstance.Resource.GetSprite(obj.ImageFilePath);

            int w = (int)(group.Texture.Width*Math.Abs(obj.Scale.X));
            int h = (int)(group.Texture.Height*Math.Abs(obj.Scale.Y));

            Vector2 anchor = new Vector2(obj.Anchor.X, obj.Anchor.Y)+new Vector2(0.5f, 0.5f);
            anchor.X*=w;
            anchor.Y*=h;

            Vector2[] vertices = new Vector2[4];

            vertices[0].X=0;
            vertices[0].Y=0;

            vertices[1].X=w;
            vertices[1].Y=0;

            vertices[2].X=w;
            vertices[2].Y=h;

            vertices[3].X=0;
            vertices[3].Y=h;

            float cosa = (float)Math.Cos(obj.Rotate*DEG2RAD);
            float sina = (float)Math.Sin(obj.Rotate*DEG2RAD);

            for (int i = 0; i<vertices.Length; i++)
            {
                var v = vertices[i]-anchor;
                v.X=v.X*cosa+v.Y*sina;
                v.Y=v.X*sina-v.Y*cosa;
                v+=obj_pos;
                points[i]=new Vector3(mouse_point-v);
            }

            Vector3 v1 = Vector3.Cross(points[0], points[1]).Normalized();
            Vector3 v2 = Vector3.Cross(points[1], points[2]).Normalized();
            Vector3 v3 = Vector3.Cross(points[2], points[3]).Normalized();
            Vector3 v4 = Vector3.Cross(points[3], points[0]).Normalized();

            if (Vector3.Dot(v1, v2)>0.9999f&&Vector3.Dot(v2, v3)>0.9999f&&
                Vector3.Dot(v3, v4)>0.9999f&&Vector3.Dot(v4, v1)>0.9999f)
                return true;
            return false;
        }

        #endregion Pick object

        public override void Term()
        {
            Window.Close();
            ToolManager.MouseClick-=OnMouseClick;
        }

        public override void Update()
        {
            Window.UpdateCurrentStoryboardObject();
        }
    }
}