using ReOsuStoryBoardPlayer.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.AutoTriggerContoller;
using ReOsuStoryBoardPlayer.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.TriggerConditionViewer
{
    public partial class TriggerConditionViewerWindow : Form
    {
        public TriggerConditionViewerWindow()
        {
            InitializeComponent();
        }

        internal void Reset()
        {
            Clear();

            var instance = StoryboardInstanceManager.ActivityInstance;

            if (instance==null)
                return;

            var list = instance.StoryboardObjectList.Where(x => x.ContainTrigger).ToArray();

            comboBox1.Items.AddRange(list);
        }

        private void Clear()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            listBox1.Items.Clear();
        }

        //选择物件
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            var obj = comboBox1.SelectedItem as StoryBoardObject;

            if (obj==null||!obj.CommandMap.TryGetValue(Event.Trigger,out var triggers))
                return;

            comboBox2.Items.Clear();

            if (triggers.Count!=0)
            {
                comboBox2.Items.AddRange(triggers.ToArray());
                comboBox2.SelectedItem=triggers.First();
                //comboBox2_SelectedValueChanged(null, null);
            }
        }

        //选择命令
        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            var obj = comboBox1.SelectedItem as StoryBoardObject;
            var command = comboBox2.SelectedItem as TriggerCommand;
            var condition = command?.Condition as HitSoundTriggerCondition;

            var hitsounds=DebuggerManager.GetDebugger<AutoTrigger>()?.objects;

            if (obj==null||hitsounds==null||hitsounds.Count==0||condition==null)
                return;

            var result = hitsounds.Where(x => command.CheckTimeVaild((float)x.Time)&&condition.CheckCondition(x));

            foreach (var item in result)
                listBox1.Items.Add(item);
        }
    }
}
