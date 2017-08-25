using SimpleRenderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public static class CommandExecutor
    {
        public delegate void CommandFunc(StoryBoardObject ref_obj, float interpolator_value, Command command);

        #region Build CommandFunctionMap

        public static Dictionary<Event,CommandFunc> CommandFunctionMap = new Dictionary<Event, CommandFunc>()
        {
            {Event.Move,Move},
            {Event.Fade,Fade},
            {Event.Color,Color},
            {Event.MoveX,MoveX},
            {Event.MoveY,MoveY},
            {Event.Scale,Scale},
            {Event.VectorScale,ScaleVector},
            {Event.Rotate,Rotate}
        };

        #endregion

        #region Command Implatements

        public static void Move(StoryBoardObject ref_obj, float current_value, Command command)
        {
            MoveCommandParameters parameters = (MoveCommandParameters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndPosition : (current_value <= 0 ? parameters.StartPostion : parameters.StartPostion + parameters.Distance * current_value);
    
            ref_obj.Postion = cal_result;
        }

        public static void MoveX(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveXCommandParameters parameters = (MoveXCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndX : (current_value <= 0 ? parameters.StartX : parameters.StartX + parameters.Distance * current_value);

            ref_obj.Postion.x = (int)cal_result;
        }

        public static void MoveY(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveYCommandParameters parameters = (MoveYCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndY : (current_value <= 0 ? parameters.StartY : parameters.StartY + parameters.Distance * current_value);

            ref_obj.Postion.y = (int)cal_result;
        }

        public static void Rotate(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            RotateCommandParamesters parameters = (RotateCommandParamesters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndRotate : (current_value <= 0 ? parameters.StartRotate : parameters.StartRotate + parameters.Distance * current_value);

            ref_obj.Rotate = cal_result;
        }

        public static void Fade(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            FadeCommandParamesters parameters = (FadeCommandParamesters)command.Parameters;
            
            float cal_result = current_value >= 1 ? parameters.EndFade : (current_value <= 0 ? parameters.StartFade : parameters.StartFade + parameters.Distance * current_value);
            
            ref_obj.Color.w = cal_result;
        }

        public static void Scale(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleCommandParameters parameters = (ScaleCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale.x=ref_obj.Scale.y= cal_result;
        }

        public static void ScaleVector(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleVectorCommandParamesters parameters = (ScaleVectorCommandParamesters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale = cal_result;
        }

        public static void Color(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ColorCommandParameters parameters = (ColorCommandParameters)command.Parameters;

            Vec4 temp = new Vec4();
            temp.x = (parameters.StartColor.x + parameters.Distance.x * current_value)/255.0f;
            temp.y = (parameters.StartColor.y + parameters.Distance.y * current_value)/255.0f;
            temp.z = (parameters.StartColor.z + parameters.Distance.z * current_value)/255.0f;

            Vec4 cal_result = current_value >= 1 ? parameters.EndColor : (current_value <= 0 ? parameters.StartColor : temp);
                
            float a = ref_obj.Color.w;
            ref_obj.Color = temp;
            ref_obj.Color.w = a;
        }

        #endregion

        public static void DispatchCommandExecute(StoryBoardObject ref_obj, uint current_playing_time, Command command)
        {
            #region Calculate interpolator value

            float current_value = command.Easing.calculate(current_playing_time- command.StartTime, command.StartTime, command.EndTime);

            #endregion

            command.executor(ref_obj, current_value, command);
        }
    }
}
