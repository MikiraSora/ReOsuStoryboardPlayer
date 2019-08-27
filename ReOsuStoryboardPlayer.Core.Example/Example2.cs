using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Example
{
    public class Example2
    {
        static void _Main(string[] args)
        {
            var simple_commands = new[]
            {
                "_M,0,0,5000,0,0,640,480",
                "_S,0,0,2500,0,1,0",
                " L,0,10",
                "__F,0,0,150,0,1",
                "__F,0,250,400,1,0"
            };

            var commands = simple_commands.Select(line => Parser.CommandParser.CommandParserIntance.Parse(line)).SelectMany(q=>q).ToList();
            var sub_command = commands.AsEnumerable().Reverse().Take(2);

            var loop_command = commands.OfType<LoopCommand>().FirstOrDefault();
            loop_command.AddSubCommand(sub_command);
            loop_command.UpdateSubCommand();

            commands = commands.Except(sub_command).ToList();

            foreach (var c in commands)
                Console.WriteLine(c.ToString());

            Console.WriteLine("Start calculate .....\n");

            StoryboardObject storyboard_object = new StoryboardObject();
            storyboard_object.ImageFilePath = "star.png";
            storyboard_object.AddCommandRange(commands);

            storyboard_object.CalculateAndApplyBaseFrameTime();

            for (int time = 0; time < 5000; time+=500)
            {
                storyboard_object.Update(time);

                Console.WriteLine($"Current time:{time} object alpha = {storyboard_object.Color.W} , position = {storyboard_object.Postion} , scale = {storyboard_object.Scale}");
            }

            Console.ReadLine();
        }
    }
}
