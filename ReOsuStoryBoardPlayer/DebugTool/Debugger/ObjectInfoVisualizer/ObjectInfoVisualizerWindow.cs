using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer
{
    public partial class ObjectVisualizerWindow : Form
    {
        private StoryBoardObject last_obj;

        public StoryBoardObject SelectObject { get; set; }

        private Dictionary<Command, TreeNode> command_node_map = new Dictionary<Command, TreeNode>();

        public ObjectVisualizerWindow(StoryBoardInstance instance)
        {
            InitializeComponent();
        }

        public void UpdateCurrentStoryboardObject()
        {
            var time = MusicPlayerManager.ActivityPlayer.CurrentTime;

            if (SelectObject != null)
            {
                if (SelectObject != last_obj)
                {
                    //这里是物件装载一次的

                    Text = SelectObject.ImageFilePath;
                    AnchorLabel.Text = SelectObject.Anchor.ToString();
                    OrderLabel.Text = SelectObject.Z.ToString();
                    TimeLabel.Text = $"{SelectObject.FrameStartTime}~{SelectObject.FrameEndTime}";
#if DEBUG
                    checkBox1.Checked=SelectObject.DebugShow;
#endif

                    command_node_map.Clear();

                    try
                    {
                        pictureBox1.Image=null;
                        var path = SelectObject.RenderGroup.ImagePath;
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

                PositionLabel.Text = SelectObject.Postion.ToString();
#if DEBUG
                SelectObject.DebugShow=checkBox1.Checked;
#endif
                int r = SelectObject.Color.x, g = SelectObject.Color.y, b = SelectObject.Color.z;
                ColorLabel.Text = $"{r},{g},{b}";
                
                try
                {
                    ColorLabel.ForeColor = Color.FromArgb(r, g, b);
                    ColorLabel.BackColor = Color.FromArgb(255 - r, 255 - g, 255 - b);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                AngleLabel.Text = (SelectObject.Rotate * 180 / Math.PI).ToString();
                AlphaLabel.Text = SelectObject.Color.w.ToString();
                ScaleLabel.Text = SelectObject.Scale.ToString();

                ParameterLabel.Text = $"{(SelectObject.IsAdditive ? "A" : " ")}{(SelectObject.IsHorizonFlip ? "H" : " ")}{(SelectObject.IsVerticalFlip ? "V" : " ")}";
                MarkdoneLabel.Text = (SelectObject.FrameStartTime<=time&&time<=SelectObject.FrameEndTime).ToString();

                UpdateCommandNode();
            }
            else
            {
                if (SelectObject != last_obj)
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

            last_obj = SelectObject;
        }

        private void ShowCommandList()
        {
            if (SelectObject != last_obj)
            {
                CommandTreeViewer.Nodes.Clear();

                if (SelectObject != null)
                {
                    var root = CommandTreeViewer.Nodes.Add(SelectObject.ToString());

                    foreach (var command_list in SelectObject.CommandMap)
                    {
                        var cmd_root = root.Nodes.Add($"{command_list.Value}");

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

        private void IsShowRawCommand_CheckedChanged(object sender, EventArgs e)
        {

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