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

namespace ReOsuStoryboardPlayer.Utils
{
    public static class ProgramUpdater
    {
        private const string RELEASE_INFO_API = @"https://api.github.com/repos/MikiraSora/ReOsuStoryboardPlayer/releases/latest";
        private const string API_USER_AGENT = "ReOsuStoryboardPlayer User";
        private const string TEMP_DIR_NAME = "update_temp";
        private const string DOWNLOAD_ZIP = "update.zip";
        private const string EXE_NAME = "ReOsuStoryBoardPlayer.exe";
        private const string UPDATE_EXE_NAME = "updater_temp.exe";


        public static async void UpdateCheck()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var program_version = asm.GetName().Version;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RELEASE_INFO_API);
                request.UserAgent=API_USER_AGENT;
                var response = await request.GetResponseAsync();

                Version release_version = null;
                string download_url = null;

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    var matches = Regex.Matches(content, @"\""(\w+)\""\s*:\s*\""(.+?)\""");

                    foreach (Match match in matches)
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
                    await Task.Run(() =>
                    {
                        if (MessageBox.Show(null, $"There is a new version available to update.", "Program Updater", MessageBoxButtons.YesNo)==DialogResult.Yes)
                        {
                            var client = new WebClient();

                            client.DownloadFile(new Uri(download_url), DOWNLOAD_ZIP);

                            if (!File.Exists(DOWNLOAD_ZIP))
                            {
                                Log.Error($"Can't download update zip file.");
                                return;
                            }

                            using (ZipArchive archive = ZipFile.Open(DOWNLOAD_ZIP, ZipArchiveMode.Read))
                            {
                                if (!Directory.Exists(TEMP_DIR_NAME))
                                    Directory.CreateDirectory(TEMP_DIR_NAME);
                                else
                                {
                                    //clean
                                    foreach (var file in Directory.EnumerateFiles(TEMP_DIR_NAME))
                                        if (Directory.Exists(file))
                                            Directory.Delete(file);
                                        else
                                            File.Delete(file);
                                }

                                archive.ExtractToDirectory(TEMP_DIR_NAME);
                            }

                            var exe_file = Directory.EnumerateFiles(TEMP_DIR_NAME, EXE_NAME).First();

                            if (!File.Exists(exe_file))
                            {
                                Log.Error($"Can't find the exe file \"{EXE_NAME}\" as program updater，please redownload or manually copy files/directories of folder \"{TEMP_DIR_NAME}\" to current program folder");
                                return;
                            }

                            var updater_exe_file = Path.Combine(TEMP_DIR_NAME, UPDATE_EXE_NAME);
                            File.Copy(exe_file, updater_exe_file, true);

                            if (File.Exists(DOWNLOAD_ZIP))
                                File.Delete(DOWNLOAD_ZIP);

                            Process.Start(new ProcessStartInfo(updater_exe_file, $"\"{Process.GetCurrentProcess().Id}\" -program_update -disable_update_check"));

                            MainProgram.Exit();
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error($"Update check failed:{e.Message} , you can go to https://github.com/MikiraSora/ReOsuStoryboardPlayer/releases/latest and check manually");
            }
        }
    
        internal static void CleanTemp()
        {
            try
            {
                if (Directory.Exists(TEMP_DIR_NAME))
                {
                    Directory.Delete(TEMP_DIR_NAME, true);
                    Log.Debug("Clean updater temp folder");
                }
            }
            catch (Exception e)
            {
                Log.Warn("Can't delete updater temp folder because "+e.Message);
            }
        }

        public static void ApplyUpdate(int raw_proc_id)
        {
            try
            {
                if (raw_proc_id!=0)
                    Process.GetProcessById(raw_proc_id)?.Kill();
            }
            catch { }

            try
            {


                Log.User($"Waiting for all players were shutdown....");

                while (Process.GetProcessesByName("ReOsuStoryBoardPlayer").Any())
                    Thread.Sleep(500);

                var current_exe_name = Path.GetFileName(Process.GetCurrentProcess().Modules[0].FileName);
                var current_path = AppDomain.CurrentDomain.BaseDirectory;

                var prev_path = new DirectoryInfo(current_path).Parent.FullName;

                bool success_fully = true;
                var files = Directory.EnumerateFiles(current_path).Where(x => Path.GetFileName(x)!=current_exe_name).ToArray();

                for (int i = 0; i<files.Length; i++)
                {
                    var source_file = files[i];
                    var display_file_path = string.Empty;

                    try
                    {
                        display_file_path=RelativePath(current_path, source_file);
                        Log.User($"Copy file({i}/{files.Length}):{display_file_path}");
                        CopyRelativeFile(source_file, current_path, prev_path);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Copy file \"{display_file_path}\" failed:{e.Message}");
                        success_fully=false;
                    }
                }

                if (success_fully)
                    Log.User("Program update successfully!");
                else
                    Log.Warn("Program update isn't successful even failed,but you can copy the files/directories of \"update_temp\" folder to current program folder");

                MainProgram.Exit("Update Done.");
            }
            catch (Exception e)
            {
                MainProgram.Exit("Update failed :"+e.Message);
            }
        }
        
        private static void CopyRelativeFile(string source_file_path, string source_root_folder, string destination_root_folder)
        {
            if (!File.Exists(source_file_path))
                return;

            var source_relative_path = RelativePath(source_root_folder, source_file_path);

            var distination_file_path = Path.Combine(destination_root_folder, source_relative_path);

            distination_file_path=Path.GetFullPath(distination_file_path);

            CreateDirectory(distination_file_path);

            File.Copy(source_file_path, distination_file_path, true);
        }

        private static void CreateDirectory(string filefullpath)
        {
            if (File.Exists(filefullpath))
            {
                return;
            }
            else
            {
                string dirpath = filefullpath.Substring(0, filefullpath.LastIndexOf('\\'));
                string[] pathes = dirpath.Split('\\');
                if (pathes.Length>1)
                {
                    string path = pathes[0];
                    for (int i = 1; i<pathes.Length; i++)
                    {
                        path+="\\"+pathes[i];
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                }
            }
        }

        private static string RelativePath(string absolutePath, string relativeTo)
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
