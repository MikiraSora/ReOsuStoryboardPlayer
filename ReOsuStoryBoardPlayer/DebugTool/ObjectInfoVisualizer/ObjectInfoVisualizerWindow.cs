using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.ObjectInfoVisualizer
{
    public partial class ObjectInfoVisualizerWindow : Form
    {
        StoryBoardObject last_obj;

        public StoryBoardObject obj { get; set; }

        public ObjectInfoVisualizerWindow()
        {
            InitializeComponent();
        }

        public void UpdateCurrentStoryboardObject()
        {
            if (obj != null)
            {
                if (obj!=last_obj)
                {
                    this.Text = obj.ImageFilePath;
                    AnchorLabel.Text = obj.Anchor.ToString();
                    OrderLabel.Text = obj.Z.ToString();
                    TimeLabel.Text = $"{obj.FrameStartTime}~{obj.FrameEndTime}";
                }

                PositionLabel.Text = obj.Postion.ToString();

                int r=(int)(obj.Color.x * 255), g=(int)(obj.Color.y * 255), b=(int)(obj.Color.z * 255);
                ColorLabel.Text = $"{r},{g},{b}";
                ColorLabel.ForeColor = Color.FromArgb(r,g,b);
                ColorLabel.BackColor = Color.FromArgb(255-r, 255 - g, 255 - b);

                AngleLabel.Text = obj.Rotate.ToString();
                AlphaLabel.Text = obj.Color.w.ToString();

                ParameterLabel.Text = $"{(obj.IsAdditive ? "A" : " ")}{(obj.IsHorizonFlip ? "H" : " ")}{(obj.IsVerticalFlip ? "V" : " ")}";
                MarkdoneLabel.Text = obj.markDone.ToString();
            }
            else
            {
                if (obj != last_obj)
                {
                    PositionLabel.Text = string.Empty;
                    ColorLabel.Text = string.Empty;
                    AngleLabel.Text = string.Empty;
                    AlphaLabel.Text = string.Empty;
                    ParameterLabel.Text = string.Empty;
                    MarkdoneLabel.Text = string.Empty;
                    AnchorLabel.Text = string.Empty;
                    TimeLabel.Text = string.Empty;
                    this.Text = string.Empty;
                    OrderLabel.Text = string.Empty;

                    ColorLabel.ForeColor = Color.White;
                    ColorLabel.BackColor = Color.White;
                }
            }

            if (IsShowRawCommand.Checked)
            {
                //ShowRawCommandList();
            }
            else
            {
                ShowCommandList();
            }

            last_obj = obj;
        }

        void ShowCommandList()
        {
            if (obj!=last_obj)
            {
                CommandTreeViewer.Nodes.Clear();

                if (obj!=null)
                {
                    var root=CommandTreeViewer.Nodes.Add(obj.ToString());
                    foreach (var command_list in obj.CommandMap)
                    {
                        var cmd_root = root.Nodes.Add(command_list.Key.ToString());
                        cmd_root.ForeColor = Color.Aqua;
                        foreach (var cmd in command_list.Value)
                        {
                            var cmd_note = cmd_root.Nodes.Add(cmd.ToString());
                            cmd_note.ForeColor = Color.LightGreen;
                        }
                    }

                    root.ExpandAll();
                }
            }
        }
        /*
        void ShowRawCommandList()
        {
            if (obj != last_obj)
            {
                CommandTreeViewer.Nodes.Clear();

                if (obj != null)
                {

                }
            }
        }*/
    }
}
