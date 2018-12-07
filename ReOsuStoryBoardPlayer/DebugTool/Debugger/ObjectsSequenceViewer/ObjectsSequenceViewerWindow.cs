using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectsSequenceViewer
{
    public partial class ObjectsSequenceViewer : Form
    {
        public struct Range
        {
            public int End;
            public int Start;

            public bool InRange(int cur) => cur>=Start&&cur<=End;
        }

        public ObjectsSequenceViewer()
        {
            InitializeComponent();

            var width_unit = (listView1.Width)/10.0f;

            ColumnHeader index_header = new ColumnHeader();
            index_header.Text="Object";
            index_header.Width=(int)(width_unit*9);
            index_header.TextAlign=HorizontalAlignment.Center;

            ColumnHeader object_header = new ColumnHeader();
            object_header.Text="Index";
            object_header.Width=(int)(width_unit*1);
            object_header.TextAlign=HorizontalAlignment.Center;

            listView1.Columns.Add(index_header);
            listView1.Columns.Add(object_header);
            listView1.Alignment=ListViewAlignment.Left;
        }

        Dictionary<ListViewItem, StoryBoardObject> registed_map = new Dictionary<ListViewItem, StoryBoardObject>();

        public void ApplyRangeFlush(Range range)
        {
            var objects = StoryboardInstanceManager.ActivityInstance.StoryboardObjectList
                .Where(o => range.InRange(o.FrameStartTime)||range.InRange(o.FrameEndTime));

            ApplyObjectsFlush(objects);
        }

        public void ApplyObjectsFlush(IEnumerable<StoryBoardObject> objects)
        {
            foreach (ListViewItem item in listView1.Items)
                ObjectPool<ListViewItem>.Instance.PutObject(item);

            listView1.Items.Clear();
            registed_map.Clear();

            var items = objects
                .Select(o => {
                    ListViewItem item = ObjectPool<ListViewItem>.Instance.GetObject();

                    item.ForeColor=o.markDone ? Color.Gray : Color.Black;
                    item.Text=o.ToString();
                    item.SubItems.Add(o.Z.ToString());

                    registed_map[item]=o;
                    return item;
                });

            listView1.BeginUpdate();
            foreach (var item in items)
                listView1.Items.Add(item);
            listView1.EndUpdate();
        }

        static Regex reg = new Regex(@"^(\d+)(([+-]{1,2})(\d+))*$");

        private void ParseFlush()
        {
            var expression = textBox1.Text.Trim();

            var match = reg.Match(expression);

            if (!match.Success)
            {
                textBox1.ForeColor=Color.Red;
                return;
            }

            textBox1.ForeColor=Color.Black;

            var base_time = match.Groups[1].Value.ToInt();

            if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                var sign = match.Groups[3].Value;
                var offset= match.Groups[4].Value.ToInt();

                Range range = new Range() { Start=base_time, End=base_time };

                if ((!sign.All(c => c=='+'||c=='-'))||sign.Distinct().Count()!=sign.Length)
                {
                    textBox1.ForeColor=Color.Red;
                    return;
                }
                else if (sign.Contains('+'))
                    range.End+=offset;
                else if (sign.Contains('-'))
                    range.Start-=offset;

                ApplyRangeFlush(range);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.HasFlag(Keys.Enter))
            {
                e.Handled=true;
                ParseFlush();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {   
            ParseFlush();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ForeColor=Color.Black;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var visualizer=DebuggerManager.GetDebugger<ObjectVisualizerDebugger>();
            var select_item = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();

            if (visualizer==null||select_item==null)
                return;

            var select_object = registed_map[select_item];

            visualizer.Window.SelectObject=select_object;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ApplyObjectsFlush(StoryboardInstanceManager.ActivityInstance.UpdatingStoryboardObjects.SelectMany(l=>l.Value).OrderBy(c=>c.Z));
        }
    }
}
