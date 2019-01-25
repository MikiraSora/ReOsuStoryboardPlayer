using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools.DefaultTools.TriggerConditionViewer;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectInfoVisualizer
{
    public partial class ObjectVisualizerWindow : Form
    {
        private StoryboardObject last_obj;

        private HashSet<TriggerCommand> trigger_status_cache = new HashSet<TriggerCommand>();

        public StoryboardObject SelectObject { get; set; }

        private Dictionary<Command, TreeNode> command_node_map = new Dictionary<Command, TreeNode>();
        private readonly StoryboardInstance instance;

        public ObjectVisualizerWindow(StoryboardInstance instance)
        {
            InitializeComponent();
            this.instance=instance;
        }

        public void UpdateCurrentStoryboardObject()
        {
            var time = MusicPlayerManager.ActivityPlayer.CurrentTime;

            if (SelectObject!=null)
            {
                if (SelectObject!=last_obj)
                {
                    //这里是物件装载一次的

                    Text=SelectObject.ImageFilePath;
                    AnchorLabel.Text=SelectObject.Anchor.ToString();
                    OrderLabel.Text=SelectObject.Z.ToString();
                    TimeLabel.Text=$"{SelectObject.FrameStartTime}~{SelectObject.FrameEndTime}";

#if DEBUG
                    checkBox1.Checked=SelectObject.DebugShow;
#endif
                    trigger_status_cache.Clear();

                    command_node_map.Clear();

                    button1.Enabled=SelectObject.ContainTrigger;

                    try
                    {
                        pictureBox1.Image=null;
                        var path = instance.Resource.GetSprite(SelectObject.ImageFilePath).ImagePath;
                        var img = Bitmap.FromFile(path);

                        if (img!=null)
                        {
                            var prev_imgae = pictureBox1.Image;
                            pictureBox1.Image=img;
                            prev_imgae.Dispose();
                        }
                    }
                    catch
                    {
                    }
                }

                //这里是要实时更新的

                PositionLabel.Text=SelectObject.Postion.ToString();
#if DEBUG
                SelectObject.DebugShow=checkBox1.Checked;
#endif
                int r = SelectObject.Color.X, g = SelectObject.Color.Y, b = SelectObject.Color.Z;
                ColorLabel.Text=$"{r},{g},{b}";

                try
                {
                    ColorLabel.ForeColor=Color.FromArgb(r, g, b);
                    ColorLabel.BackColor=Color.FromArgb(255-r, 255-g, 255-b);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                AngleLabel.Text=(SelectObject.Rotate*180/Math.PI).ToString();
                AlphaLabel.Text=SelectObject.Color.W.ToString();
                ScaleLabel.Text=SelectObject.Scale.ToString();

                ParameterLabel.Text=$"{(SelectObject.IsAdditive ? "A" : " ")}{(SelectObject.IsHorizonFlip ? "H" : " ")}{(SelectObject.IsVerticalFlip ? "V" : " ")}";
                MarkdoneLabel.Text=(SelectObject.FrameStartTime<=time&&time<=SelectObject.FrameEndTime).ToString();

                UpdateCommandNode();
            }
            else
            {
                if (SelectObject!=last_obj)
                {
                    PositionLabel.Text=string.Empty;
                    ColorLabel.Text=string.Empty;
                    AngleLabel.Text=string.Empty;
                    AlphaLabel.Text=string.Empty;
                    ParameterLabel.Text=string.Empty;
                    MarkdoneLabel.Text=string.Empty;
                    AnchorLabel.Text=string.Empty;
                    TimeLabel.Text=string.Empty;
                    this.Text=string.Empty;
                    OrderLabel.Text=string.Empty;

                    ColorLabel.ForeColor=Color.White;
                    ColorLabel.BackColor=Color.White;

                    command_node_map.Clear();
                }
            }

            ShowCommandList();

            last_obj=SelectObject;
        }

        private void ShowCommandList()
        {
            if (SelectObject!=last_obj)
            {
                CommandTreeViewer.Nodes.Clear();

                if (SelectObject!=null)
                {
                    var root = CommandTreeViewer.Nodes.Add(SelectObject.ToString());

                    foreach (var command_list in SelectObject.CommandMap)
                    {
                        var cmd_root = root.Nodes.Add($"{command_list.Value}");

                        foreach (var cmd in command_list.Value)
                        {
                            var cmd_note = cmd_root.Nodes.Add(cmd.ToString());

                            if (cmd is GroupCommand group_command)
                            {
                                foreach (var loop_sub_command in group_command.SubCommands.SelectMany(l => l.Value).OrderBy(c => c.StartTime))
                                {
                                    var loop_sub_note = cmd_note.Nodes.Add(loop_sub_command.ToString());
                                    BindCommandNode(loop_sub_command, loop_sub_note);
                                }
                            }

                            BindCommandNode(cmd, cmd_note);
                        }
                    }

                    root.ExpandAll();
                }
            }

            void BindCommandNode(Command command, TreeNode node)
            {
                command_node_map[command]=node;
            }
        }

        private void UpdateCommandNode()
        {
            foreach (var pair in command_node_map)
            {
#if DEBUG
                pair.Value.BackColor=pair.Key.IsExecuted ? Color.Aqua : Color.Transparent;
#endif
                if (pair.Key is TriggerCommand trigger)
                {
                    //这里必须做内容更新判断否则因为太过频繁窗口会出现闪烁现象
                    if (trigger.Trigged!=trigger_status_cache.Contains(trigger))
                    {
                        if (trigger.Trigged)
                            trigger_status_cache.Add(trigger);
                        else
                            trigger_status_cache.Remove(trigger);

                        try
                        {
                            //偶尔莫名其妙跳一个Disposed异常
                            pair.Value.Text=trigger.ToString();
                        }
                        catch { }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var debugger = ToolManager.GetOrCreateTool<TriggerConditionViewerDebugger>();

            if (SelectObject?.ContainTrigger??false)
            {
                debugger.Window.ExcplictSelect(SelectObject);

                debugger.Window.Visible=true;
            }
        }
    }
}