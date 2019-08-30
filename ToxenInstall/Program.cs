using System;
using System.Net;
using System.Collections.Generic;
using Octokit;
using System.Threading.Tasks;
using System.Threading;
using IonLib.Network;
using System.IO;
using System.IO.Compression;

namespace ToxenUpdate
{
    class Program
    {
        static WebClient wc = new WebClient();
        static GitHubClient github = new GitHubClient(new ProductHeaderValue("Toxen"));
        static string toxenPath = "C:/Toxen/";
        static string toxenFile = "ToxenLatest.zip";
        static void Main(string[] args)
        {
            if (args[0] == "-here")
            {
                goto startDownload;
            }
        start:
            Console.WriteLine("Do you want to use \"" + toxenPath + "\" as your directory? (yes/no):");
            //Make current directory... maybe..
            //Console.WriteLine("Do you want to use current directory as your directory? (yes/no):");
            Console.Write("> ");
            string useDefault = Console.ReadLine();
            if (useDefault == "y" || useDefault == "yes")
            {

            }
            else if (useDefault == "n" || useDefault == "no")
            {
                Console.WriteLine("What path would you like to install to?:");
                Console.Write("> ");
                toxenPath = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("I didn't understand that, try again\n");
                goto start;
            }
            startDownload:
            toxenPath = toxenPath.Replace("\\", "/");
            if (!toxenPath.EndsWith("/"))
            {
                toxenPath += "/";
            }
            Install();
            //ZipFile.ExtractToDirectory(toxenPath + toxenFile, toxenPath);
            ZipArchive filelist = ZipFile.OpenRead(toxenPath + toxenFile);
            foreach (ZipArchiveEntry item in filelist.Entries)
            {
                string saveToPath = Path.Combine(toxenPath, item.FullName);
                string pathFromFile = saveToPath.Substring(0, saveToPath.Length - saveToPath.Split('/')[saveToPath.Split('/').Length - 1].Length);
                Console.WriteLine(saveToPath);
                Console.WriteLine(pathFromFile);
                if (saveToPath.EndsWith("settings.json"))
                {
                    try
                    {
                        item.ExtractToFile(Path.Combine(toxenPath, item.FullName), true);
                    }
                    catch (Exception)
                    {
                        if (!Directory.Exists(pathFromFile))
                        {
                            Console.WriteLine("Creating directory: " + pathFromFile);
                            Directory.CreateDirectory(pathFromFile);
                        }
                    }
                }
            }
            filelist.Dispose();
            File.Delete(toxenPath + toxenFile);
            Console.WriteLine("Update completed");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        static void Install()
        {
            IReadOnlyList<Release> releases;
            try
            {
                releases = GetReleases("LucasionGS", "Toxen").Result;
                Console.WriteLine("Newest release: " + releases[0].TagName);
                Console.WriteLine(releases[0].Assets[0].BrowserDownloadUrl);
                DL.DLDirectory(toxenPath);
                DL.AddDlList(new string[] { releases[0].Assets[0].BrowserDownloadUrl, toxenFile }, true);
                Dictionary<string, int> files = DL.StartDownload();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong...\nTry to restart");
                Console.WriteLine(e.Message);
            }
        }


        /// <summary>
        /// Get Github repository
        /// </summary>
        /// <param name="username">Author of the repository</param>
        /// <param name="repository">Repository name</param>
        /// <returns></returns>
        async static Task<IReadOnlyList<Release>> GetReleases(string username, string repository)
        {
            Task<IReadOnlyList<Release>> releasesAsync;
            IReadOnlyList<Release> releases;
            try
            {
                //jsonString = wc.DownloadString("https://api.github.com/repos/LucasionGS/Toxen/releases/latest");
                releasesAsync = github.Repository.Release.GetAll(username, repository);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            releases = await releasesAsync;
            return releases;
        }
    }
}
