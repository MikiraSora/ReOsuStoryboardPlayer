using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryBroadParser
{
    public class Parser
    {
        public static bool isSpriteLine(string text)
        {
            return text.StartsWith("Sprite");
        }

        public static Command tryParseCommand(string commandText)
        {
            try
            {
                Command command = new Command();
                string[] command_params = commandText.Trim().Split(splitChars);

                command._event = String2Event(command_params[0]);
                command._easing = String2Easing(command_params[1]);
                command._startTime = Int32.Parse(command_params[2]);

                if (command_params[3].Trim() != string.Empty)
                    command._endTime = Int32.Parse(command_params[3]);
                else
                    command._endTime = -1000 ;

                command._params = new List<string>();
                for (int i = 4; i < command_params.Length; i++)
                    command._params.Add(command_params[i]);

                if (command._event == Events.Color && command._params.Count == 3)
                {
                    command._params.Add(command._params[0]);
                    command._params.Add(command._params[1]);
                    command._params.Add(command._params[2]);
                    //command._params.Add("255");
                    //command._params.Add("255");
                    //command._params.Add("255");
                }
                else if (command._event == Events.Rotate && command._params.Count == 1)
                {
                    //command._params.Add("0");
                    command._params.Add(command._params[0]);
                }
                else if (command._event == Events.Scale && command._params.Count == 1)
                {
                    //command._params.Add("1");//old
                    command._params.Add(command._params[0]);
                }
                else if (command._event == Events.Fade && command._params.Count == 1)
                {
                    //command._params.Add("1");
                    command._params.Add(command._params[0]);
                }
                else if (command._event == Events.VectorScale && command._params.Count == 2)
                {
                    //command._params.Add("1");
                    //command._params.Add("1");
                    command._params.Add(command._params[0]);
                    command._params.Add(command._params[1]);
                }
                else if (command._event == Events.Move && command._params.Count == 2)
                {
                    command._params.Add(command._params[0]);
                    command._params.Add(command._params[1]);
                }
                else if ((command._event == Events.MoveX|| command._event == Events.MoveY) && command._params.Count == 1)
                {
                    command._params.Add(command._params[0]);
                }

                    return command;
            }
            catch (Exception e)
            {
                Console.WriteLine("Command \"{0}\" parse error!{1}", commandText, e.Message);
                return null;
            }
        }

        public static bool isCommandLine(string text)
        {
            return text.Length==0?false:text[0] == ' ' || text[0] == '_';
        }

        static char[] splitChars = new char[] {','};

        public static Sprite tryParseSprite(string spriteText)
        {
            try
            {
                Sprite sprite = new Sprite();

                string[] sprite_params = spriteText.Trim().Split(splitChars);

                sprite._layer = (Layer)Enum.Parse(typeof(Layer),sprite_params[1]);
                sprite._origin = (Origin)Enum.Parse(typeof(Origin), sprite_params[2]);
                sprite._imgPath = sprite_params[3].Substring(1, sprite_params[3].Length-2);

                sprite._x = Int32.Parse(sprite_params[4]);
                sprite._y = Int32.Parse(sprite_params[5]);

                return sprite;
            }
            catch (Exception e)
            {
                Console.WriteLine("Sprite \"{0}\" parse error!{1}", spriteText, e.Message);
                return null;
            }
        }

        public static void recheckCommand(ref Sprite spriteObject)
        {
            var commands = spriteObject._commands;
            int min = Int32.MaxValue, max = 0 ;

            bool[] isFirst = new bool[11];

            foreach (var command in commands) {
                if (command._startTime < min && command._endTime != -1000)
                    min = command._startTime;

                if (command._endTime > max && command._endTime != -1000)
                    max = command._endTime;
            }

            foreach(var command in commands)
            {
                if(command._endTime == -1000&&!isFirst[(int)command._event]&&(command._event==Events.Move))
                {
                    isFirst[(int)command._event] = true;
                    command._startTime = min;
                    //command._endTime = max;
                    command._endTime = min;
                }
            }
        }

        static Events String2Event(String str)
        {
            if (str[0] == '_')
            {
                str=str.Substring(1);
            }
            switch (str.Trim())
            {
                case "F":
                    return Events.Fade;
                case "P":
                    return Events.Parameter;
                case "MX":
                    return Events.MoveX;
                case "MY":
                    return Events.MoveY;
                case "S":
                    return Events.Scale;
                case "R":
                    return Events.Rotate;
                case "V":
                    return Events.VectorScale;
                case "C":
                    return Events.Color;
                case "M":
                    return Events.Move;
                case "T":
                    return Events.Trigger;
                case "L":
                    return Events.Loop;

                default:
                    throw new Exception(string.Format("Unknown event type \"{0}\" ",str));
            }
        }

        static Easing String2Easing(string str)
        {
            return (Easing)Int32.Parse(str);
        }

        public static List<Sprite> parseStrings(string[] strArray)
        {
            StoryBroadParser.Sprite sprite;
            Command command;
            LoopCommand loopCommand = null;
            string buffer;
            List<StoryBroadParser.Sprite> spriteList = new List<StoryBroadParser.Sprite>();

            for (int i = 0; i < strArray.Length; i++)
            {

                buffer = strArray[i];

                if (Parser.isSpriteLine(buffer))
                {
                    sprite = Parser.tryParseSprite(buffer);
                    if (sprite != null)
                    {

                        if (spriteList.Count != 0)
                        {
                            var prev_sprite = spriteList[spriteList.Count - 1];
                            Parser.recheckCommand(ref prev_sprite); //BUG
                        }

                        //add new sprite and clear
                        spriteList.Add(sprite);
                    }
                    continue;
                }

                if (Parser.isCommandLine(buffer))
                {
                    command = Parser.tryParseCommand(buffer);
                    if (command != null)
                    {
                        if (buffer[1] == ' ')
                        {
                            if (loopCommand == null)
                                throw new Exception("Cant add sub command into null loopCommand");

                            loopCommand._loopCommandList.Add(command);
                        }
                        else
                        {
                            spriteList[spriteList.Count - 1]._commands.Add(command);
                        }
                    }
                    if (command is LoopCommand)
                    {
                        loopCommand = (LoopCommand)command;
                    }
                    continue;
                }
            }

            return spriteList;
        }
    }
}
