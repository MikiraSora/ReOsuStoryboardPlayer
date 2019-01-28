using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net.Http;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ProgramUpdater
{
    public class ProgramUpdateCheck : ToolBase
    {
        private const string RELEASE_INFO_API = @"https://api.github.com/repos/MikiraSora/ReOsuStoryboardPlayer/releases";

        public override void Init()
        {
            try
            {
                UpdateCheck();
            }
            catch (Exception e)
            {
                Log.Error($"Update check failed because {e.Message}");
            }
        }

        private async void UpdateCheck()
        {
            var asm = Assembly.GetExecutingAssembly();
            var program_version = asm.GetName().Version;

            var request = HttpWebRequest.Create(RELEASE_INFO_API);
            var response = await request.GetResponseAsync();

            Version release_version=null;
            string download_url = null;

            using (var reader=new StreamReader(response.GetResponseStream()))
            {
                var content = reader.ReadToEnd();
                var match = Regex.Match(content, @"\""(\w+)\""\s*:\s*\""(.+)\""");

                while (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var val = match.Groups[2].Value;

                    switch (name.ToLower())
                    {
                        case "tag_name":
                            release_version=new Version(val.TrimStart('v'));
                            break;
                        case "browser_download_url":
                            download_url=val;
                            break;
                        default:
                            break;
                    }

                    match=match.NextMatch();
                }
            }

            if (release_version==null||string.IsNullOrWhiteSpace(download_url))
            {
                Log.Warn("Get release info failed! please visit \"https://github.com/MikiraSora/ReOsuStoryboardPlayer/releases/latest\".");
                return;
            }

            Log.User($"Progress current version:{program_version} ,release latest version:{release_version}");

            if (release_version>program_version)
            {
                if (MessageBox.Show($"There is a new version available to update.", "Program Updater", MessageBoxButtons.YesNo)==DialogResult.Yes)
                {
                    var client = new WebClient();

                    var update_file_name = "update.zip";

                    if (File.Exists(update_file_name))
                        File.Delete(update_file_name);

                    client.DownloadFile(new Uri(download_url), update_file_name);

                    if (!File.Exists(update_file_name))
                    {
                        Log.Error($"Can't download update zip file.");
                        return;
                    }

                    var temp_dir_name = "update_temp";

                    using (ZipArchive archive = ZipFile.Open(update_file_name, ZipArchiveMode.Read))
                    {
                        if (!Directory.Exists(temp_dir_name))
                            Directory.CreateDirectory(temp_dir_name);
                        else
                        {
                            //clean
                            foreach (var file in Directory.EnumerateFiles(temp_dir_name))
                                if (Directory.Exists(file))
                                    Directory.Delete(file);
                                else
                                    File.Delete(file);
                        }

                        archive.ExtractToDirectory(temp_dir_name);
                    }

                    var exe_name = "ReOsuStoryBoardPlayer.exe";
                    var exe_file = Directory.EnumerateFiles(temp_dir_name, exe_name).First();

                    if (!File.Exists(exe_file))
                    {
                        Log.Error($"找不到{exe_name}作为更新启动器，请重新下载或者去复制{temp_dir_name}文件夹内容,覆盖到本程序根目录手动更新");
                        return;
                    }

                    var updater_exe_file = Path.Combine(temp_dir_name, "updater_temp.exe");
                    File.Copy(exe_file, updater_exe_file);

                    Process.Start(new ProcessStartInfo(updater_exe_file, $"\"{Process.GetCurrentProcess().Id}\" -program_update"));

                    MainProgram.Exit();
                }
            }
        }

        public void ApplyUpdate(int raw_proc_id)
        {
            if (raw_proc_id!=0)
                Process.GetProcessById(raw_proc_id)?.Kill();

            Log.User($"Waiting for all players were shutdown....");

            while (Process.GetProcessesByName("ReOsuStoryBoardPlayer").Any())
                Thread.Sleep(500);

            var current_exe_name=Path.GetFileName(Process.GetCurrentProcess().Modules[0].FileName);
            var current_path = AppDomain.CurrentDomain.BaseDirectory;

            var prev_path = Directory.GetParent(current_path).FullName;

            foreach (var source_file in Directory.EnumerateFiles(current_path).Where(x=>Path.GetFileName(x)!=current_exe_name))
            {
                File.Copy(source_file,)
            }

            Log.User("Program update successfully!");
            Thread.Sleep(2000);
            MainProgram.Exit();
        }

        public override void Term()
        {

        }

        public override void Update()
        {

        }

        public static string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');
            
            int length = absoluteDirectories.Length<relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;
            
            int lastCommonRoot = -1;
            int index;
            
            for (index=0; index<length; index++)
                if (absoluteDirectories[index]==relativeDirectories[index])
                    lastCommonRoot=index;
                else
                    break;
            
            if (lastCommonRoot==-1)
                throw new ArgumentException("Paths do not have a common base");
            
            StringBuilder relativePath = new StringBuilder();
            
            for (index=lastCommonRoot+1; index<absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length>0)
                    relativePath.Append("..\\");
            
            for (index=lastCommonRoot+1; index<relativeDirectories.Length-1; index++)
                relativePath.Append(relativeDirectories[index]+"\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length-1]);

            return relativePath.ToString();
        }
    }
}
