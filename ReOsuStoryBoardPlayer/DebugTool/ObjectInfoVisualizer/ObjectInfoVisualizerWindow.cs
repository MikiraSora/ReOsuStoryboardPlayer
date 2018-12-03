using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.ObjectInfoVisualizer
{
    public partial class ObjectInfoVisualizerWindow : Form
    {
        private StoryBoardObject last_obj;

        public StoryBoardObject obj { get; set; }
        public StoryBoardInstance Instance { get; }

        private Dictionary<Command, TreeNode> command_node_map = new Dictionary<Command, TreeNode>();

        public ObjectInfoVisualizerWindow(StoryBoardInstance instance)
        {
            InitializeComponent();
            Instance = instance;
        }

        public void UpdateCurrentStoryboardObject()
        {
            if (obj != null)
            {
                if (obj != last_obj)
                {
                    //这里是物件装载一次的

                    Text = obj.ImageFilePath;
                    AnchorLabel.Text = obj.Anchor.ToString();
                    OrderLabel.Text = obj.Z.ToString();
                    TimeLabel.Text = $"{obj.FrameStartTime}~{obj.FrameEndTime}";

                    command_node_map.Clear();

                    try
                    {
                        var path = Path.Combine(Instance.folder_path, obj.ImageFilePath);
                        var img = Bitmap.FromFile(path);

                        if (img != null)
                        {
                            var prev_imgae = pictureBox1.Image;
                            pictureBox1.Image = img;
                            prev_imgae.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("[ObjectInfoVisualizer]Can't load bitmap:" + e.Message);
                    }
                }

                //这里是要实时更新的

                PositionLabel.Text = obj.Postion.ToString();

                int r = (int)(obj.Color.x * 255), g = (int)(obj.Color.y * 255), b = (int)(obj.Color.z * 255);
                ColorLabel.Text = $"{r},{g},{b}";
                ColorLabel.ForeColor = Color.FromArgb(r, g, b);
                ColorLabel.BackColor = Color.FromArgb(255 - r, 255 - g, 255 - b);

                AngleLabel.Text = (obj.Rotate * 180 / Math.PI).ToString();
                AlphaLabel.Text = obj.Color.w.ToString();
                ScaleLabel.Text = obj.Scale.ToString();

                ParameterLabel.Text = $"{(obj.IsAdditive ? "A" : " ")}{(obj.IsHorizonFlip ? "H" : " ")}{(obj.IsVerticalFlip ? "V" : " ")}";
                MarkdoneLabel.Text = obj.markDone.ToString();

                UpdateCommandNode();
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

                    command_node_map.Clear();
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

        private void ShowCommandList()
        {
            if (obj != last_obj)
            {
                CommandTreeViewer.Nodes.Clear();

                if (obj != null)
                {
                    var root = CommandTreeViewer.Nodes.Add(obj.ToString());

                    foreach (var command_list in obj.CommandMap)
                    {
                        var cmd_root = root.Nodes.Add(command_list.Key.ToString());
                        cmd_root.ForeColor = Color.AliceBlue;

                        foreach (var cmd in command_list.Value)
                        {
                            var cmd_note = cmd_root.Nodes.Add(cmd.ToString());

                            if (cmd is LoopCommand loop_command)
                            {
                                foreach (var loop_sub_command in loop_command.SubCommands.SelectMany(l => l.Value).OrderBy(c => c.StartTime))
                                {
                                    var loop_sub_note = cmd_note.Nodes.Add(loop_sub_command.ToString());
                                    BindCommandNode(loop_sub_command, loop_sub_note);
                                }
                            }
                            else
                                BindCommandNode(cmd, cmd_note);
                        }
                    }

                    root.ExpandAll();
                }
            }

            void BindCommandNode(Command command, TreeNode node)
            {
                command_node_map[command] = node;
            }
        }

        private void UpdateCommandNode()
        {
#if DEBUG
            foreach (var pair in command_node_map)
            {
                pair.Value.BackColor = pair.Key.IsExecuted ? Color.Aqua : Color.Transparent;
            }
#endif
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