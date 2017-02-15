using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StoryBroadParser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Sprite> spriteList = new List<Sprite>();
            Sprite sprite;
            Command command;

            string filePath = /*args[1]*/@"G:\osu!\Songs\292301 xi - Blue Zenith\xi - Blue Zenith (Asphyxia).osb";
            string buffer;

            int totalCommands = 0;
            
            StreamReader reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                buffer = reader.ReadLine();

                if(Parser.isSpriteLine(buffer))
                {
                    sprite = Parser.tryParseSprite(buffer);
                    if (sprite != null)
                        spriteList.Add(sprite);
                    continue;
                }

                if (Parser.isCommandLine(buffer))
                {
                    command = Parser.tryParseCommand(buffer);
                    if (command != null)
                    {
                        spriteList[spriteList.Count - 1]._commands.Add(command);
                        totalCommands++;
                    }
                    continue;
                }

            }

            Console.WriteLine("parse finish! ,now there are {0} sprites and {1} commands",spriteList.Count,totalCommands);
            Console.ReadLine();
        }
    }
}
