using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGLF;
using OpenTK.Input;
using IrrKlang;
using OpenTK;
using System.IO;
using System.Threading.Tasks;
using StoryBroadParser;

namespace OsuStoryBroadPlayer
{
    class Program
    {
        class StoryBroadPlayer : Window
        {
            string _oszPath;

            StoryBroadInitializer initializer;

            string _mp3FilePath, _osbFilePath,_osuFilePath;

            ISoundSource mp3PlayerSource;
            ISound player;

            public StoryBroadPlayer(string oszPath,string mp3FilePath,string osbFilePath,string osuFilePath):base(640,460)
            {
                string buffer;

                _osbFilePath = osbFilePath;
                _oszPath = oszPath;
                _mp3FilePath = mp3FilePath;
                _osuFilePath = osuFilePath;

                StreamReader reader;

                List<string> strList = new List<string>();

                List<StoryBroadParser.Sprite> spriteList = new List<StoryBroadParser.Sprite>();

                if (_osuFilePath != null)
                {
                    using (reader = new StreamReader(_osuFilePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            buffer = reader.ReadLine();
                            strList.Add(buffer);
                        }
                    }
                }

                using (reader = new StreamReader(osbFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        buffer = reader.ReadLine();

                        strList.Add(buffer);
                    }
                }

                spriteList = Parser.parseStrings(strList.ToArray());

                initializer = new StoryBroadInitializer(_oszPath, spriteList);
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                Engine.scene = new Scene();

                //Engine.debugGameObject = true;

                var list=initializer.Genarate();

                Schedule.addMainThreadUpdateTask(new Schedule.ScheduleTask(1000, true, null, -1, (refTask, param) =>
                {
                    for(int i = 0; i < list.Count; i++)
                    {
                        var sprite = list[i];

                        if (player.PlayPosition + 1000 > sprite._startTime)
                        {
                            Log.User("playback:{0},show {1} sprite in {2}",player.PlayPosition,sprite.name,sprite._startTime);
                            Engine.scene.GameObjectRoot.addChild(sprite);
                            list.RemoveAt(i);
                            i--;
                        }
                    }

                    if (list.Count == 0)
                        refTask.markLoopDone();
                }));

                //miss plugin
                mp3PlayerSource = Engine.sound.AddSoundSourceFromFile(_mp3FilePath);
                player=Engine.sound.Play2D(mp3PlayerSource,false,false,true);

                initializer.SetPlayer(player);
            }

           
            protected override void OnMouseDown(MouseButtonEventArgs e)
            {
                base.OnMouseDown(e);
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base.OnUpdateFrame(e);
                Title = string.Format("StoryBoard Player time:{0} \t fps:{1} sprite:{2}", player.PlayPosition , Math.Truncate(UpdateFrequency),Engine.scene.GameObjectRoot.getChildren().Count);
            }
            int i = 0;
            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                base.OnKeyPress(e);
            }
        }

        public static void Main(string[] argv)
        {
            //path
            string oszPath = (argv.Length==1?argv[0]:@"372552 yuiko - Azuma no Sora kara Hajimaru Sekai (Short)") + @"\";
            string osbFilePath="", osuSufFilePath="", musicFilePath="";
            foreach(string path in Directory.GetFiles(oszPath))
            {
                if (path.EndsWith(@".mp3"))
                {
                    musicFilePath = path;
                    continue;
                }
                if (path.EndsWith(@".osb"))
                {
                    osbFilePath = path;
                    continue;
                }
                if (path.EndsWith(@".osu"))
                {
                    osuSufFilePath = path;
                }
            }

            Console.WriteLine("Path:{0}\nosuFilePath :{1}\nStoryBoardFilePath:{2}\nmusicFilePath:{3}",oszPath,osuSufFilePath,osbFilePath,musicFilePath);

            if (osuSufFilePath.Length * osbFilePath.Length * musicFilePath.Length==0)
            {
                Console.WriteLine("Missing osuFile/mp3File/osbFile!");
                Environment.Exit(-1);
            }

            StoryBroadPlayer player = new StoryBroadPlayer(oszPath,musicFilePath,osbFilePath,osuSufFilePath);
            player.Run();
        }
    }
}
