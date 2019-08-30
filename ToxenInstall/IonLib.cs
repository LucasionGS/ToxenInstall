using System.IO;
using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web;

namespace IonLib
{
    public static class Base
    {
        /// <summary>
        /// Writes in colored text.
        /// </summary>
        /// <param name="text">The text to display as WriteLine</param>
        /// <param name="color">The color to write in</param>
        public static void WriteInColor(string text, ConsoleColor color)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = preColor;
        }
        /// <summary>
        /// Writes in colored text and a newline.
        /// </summary>
        /// <param name="text">The text to display as WriteLine</param>
        /// <param name="color">The color to write in</param>
        public static void WriteLineInColor(string text, ConsoleColor color)
        {
            WriteInColor(text, color);
            Console.WriteLine();
        }

        #region Error Functions
        public static void Error()
        {
            WriteLineInColor("An error has occurred", ConsoleColor.Red);
        }
        public static void Error(string message)
        {
            WriteLineInColor(message, ConsoleColor.Red);
        }
        #endregion
    }

    // Functions with ReadLine properties
    namespace ReadLine
    {
        public static class ReadLine
        {
            /// <summary>
            /// Convert a Console.ReadLine output into an integer directly.
            /// Anything which isn't an integer will be declined.
            /// </summary>
            /// <param name="failureMsg">
            /// An optional error message for when the user writes something that isn't an integer
            /// </param>
            /// <returns>An integer which has been converted from a Console.ReadLine</returns>
            public static int ToInt(string failureMsg = "")
            {
                int answer;
                while (!int.TryParse(Console.ReadLine(), out answer))
                {
                    if (failureMsg != "")
                    {
                        Console.WriteLine();
                    }
                }
                return answer;
            }
            /// <summary>
            /// Convert a Console.ReadLine output into an integer directly.
            /// Anything which isn't an integer will be declined.
            /// </summary>
            /// <param name="failureMsg">
            /// An error message for when the user writes something that isn't an integer
            /// </param>
            /// <param name="color">
            /// The color you want the error text to be written in.
            /// </param>
            /// <returns>An integer which has been converted from a Console.ReadLine</returns>
            public static int ToInt(string failureMsg, ConsoleColor color)
            {
                int answer;
                while (!int.TryParse(Console.ReadLine(), out answer))
                {
                    Base.WriteLineInColor(failureMsg, color);
                }
                return answer;
            }
        }
    }

    // A file downloader
    namespace Network
    {
        class DL
        {
            public enum BytePreference
            {
                Auto,
                Bytes,
                KiloBytes,
                MegaBytes,
                GigaBytes
            }
            static char[] illegalChars = "\\:*?\"<>|".ToCharArray(); //doesn't include /
            static string FilterIllegalChars(string _text, string replaceWith = "_")
            {
                for (int i = 0; i < illegalChars.Length; i++)
                {
                    _text = _text.Replace(illegalChars[i].ToString(), replaceWith);
                }
                return _text;
            }
            public static bool silentDownload = false;
            public static bool forceDownload = false;
            static string rootUrl = "";
            public static void RootUrl(string _rootUrl)
            {
                rootUrl = _rootUrl;
            }
            static string dlDirectory = "C:/";
            public static void DLDirectory(string directory)
            {
                dlDirectory = directory;
            }
            static BytePreference bytePreference = BytePreference.Auto;
            /// <summary>
            /// Set the Byte preference
            /// </summary>
            public static void BytePref(BytePreference bp)
            {
                bytePreference = bp;
            }
            public static int procent = -1;
            public static long curbytes, totalBytes;
            readonly static WebClient webClient = new WebClient();
            static Uri url;
            //public static List<string> downloadList = new List<string>();
            public static List<string[]> downloadList = new List<string[]>();
            //Add SetDlList Support for
            //  Giving each download their own custom name.
            //  Potentionally use List<string[]>
            //  a string array of 2, 1st being the URL, 2nd being the file name.

            //Also add support for
            //  Reading a local file with URL's
            //  and maybe also with custom names if i add support for that
            #region SetDlList() overloads without custom names
            public static void SetDlList(string url)
            {
                ResetDlList();
                AddDlList(url);
            }
            public static void SetDlList(string[] urls)
            {
                ResetDlList();
                AddDlList(urls);
            }

            public static void AddDlList(string url)
            {
                downloadList.Add(new string[] { url, RemoveRoot(url) });
            }
            public static void AddDlList(string[] urls)
            {
                string[][] toAdd = new string[urls.Length][];
                for (int i = 0; i < urls.Length; i++)
                {
                    toAdd[i] = new string[] { urls[i], RemoveRoot(urls[i]) };
                }
                downloadList.AddRange(toAdd);
            }

            public static void ResetDlList()
            {
                downloadList.Clear();
            }
            #endregion
            #region SetDlList() overloads with custom names
            public static void SetDlList(string[] url, bool customNames)
            {
                if (!customNames)
                {
                    SetDlList(url[0]);
                }
                else
                {
                    ResetDlList();
                    downloadList.Add(url);
                }
            }
            public static void SetDlList(string[][] urls)
            {
                ResetDlList();
                downloadList.AddRange(urls);
            }

            public static void AddDlList(string[] url, bool customNames)
            {
                if (!customNames)
                {
                    AddDlList(url[0]);
                }
                else
                {
                    downloadList.Add(url);
                }
            }
            public static void AddDlList(string[][] urls)
            {
                downloadList.AddRange(urls);
            }
            #endregion

            //Create directories and all sub directories if they don't exist
            public static void CreateDirectory(string directory)
            {
                directory = directory.Replace("\\", "/");
                string[] _dirs = directory.Split('/');
                List<string> dirs = new List<string>(_dirs);
                string curDir = dirs[0] + "/";
                dirs.RemoveAt(0);
                while (!Directory.Exists(directory))
                {
                    if (Directory.Exists(curDir))
                    {
                        curDir += dirs[0] + "/";
                        dirs.RemoveAt(0);
                    }
                    else
                    {
                        Directory.CreateDirectory(curDir);
                    }
                }
            }

            //Get a string from a page
            public static string GetString(string url)
            {
                return webClient.DownloadString(url);
            }
            //Get a string from a page as array
            public static string[] GetStringArray(string url)
            {
                List<string> list = new List<string>(webClient.DownloadString(url).Split('\n'));
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == "")
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
                return list.ToArray();
            }

            //Download function to start the downloading.
            //Used in StartDownload()
            static void DownloadFile(string url, string directory)
            {
                DL.url = new Uri(url);
                string fileName = GetFileName(url);
                fileName = FilterIllegalChars(fileName);
                if (!Directory.Exists(directory))
                {
                    CreateDirectory(directory);
                }
                webClient.DownloadFileAsync(DL.url, directory + "/" + fileName);
            }
            static void DownloadFile(string url, string directory, string fileName)
            {
                DL.url = new Uri(url);
                fileName = FilterIllegalChars(fileName);
                if (!Directory.Exists(directory))
                {
                    CreateDirectory(directory);
                }
                webClient.DownloadFileAsync(DL.url, directory + "/" + fileName);
            }

            static string GetFileName(string url)
            {
                string[] urlArgs = url.Split('/');
                return WebUtility.UrlDecode(urlArgs[urlArgs.Length - 1]);
            }
            static string RemoveRoot(string url)
            {
                return "/" + HttpUtility.UrlDecode(url.Substring(rootUrl.Length));
            }

            /// <summary>
            /// Start Download from downloadList
            /// </summary>
            public static Dictionary<string, int> StartDownload()
            {
                Dictionary<string, int> newFiles = new Dictionary<string, int>
                {
                    { "new", 0 },
                    { "updated", 0 },
                    { "exists", 0 },
                    { "failed", 0 }
                };
                webClient.DownloadDataCompleted += WebClient_DownloadDataCompleted;
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                int preProcent = -1;
                while (true)
                {
                    if (!webClient.IsBusy)
                    {
                        if (procent != -1)
                        {
                            ConsoleColor _c = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n | Completed!\n");
                            Console.ForegroundColor = _c;
                        }
                        if (downloadList.Count > 0)
                        {
                            string fileName = downloadList[0][1];
                            string fullFileName = dlDirectory + "/" + fileName;
                            FileInfo fileInfo = new FileInfo(fullFileName);
                            try
                            {
                                webClient.OpenRead(downloadList[0][0]);
                                if (!forceDownload && File.Exists(fullFileName) && fileInfo.Length == Convert.ToInt64(webClient.ResponseHeaders["Content-Length"]))
                                {
                                    if (!silentDownload)
                                    {
                                        ConsoleColor _c = Console.ForegroundColor;
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("File already in folder: " + fullFileName);
                                        Console.ForegroundColor = _c;
                                        newFiles["exists"]++;
                                        procent = -1;
                                    }
                                }
                                else
                                {
                                    if (!forceDownload && File.Exists(fullFileName))
                                    {
                                        newFiles["updated"]++;
                                        if (!silentDownload) Console.WriteLine("Updating \"" + fileName + "\" from " + downloadList[0][0]);
                                    }
                                    else
                                    {
                                        newFiles["new"]++;
                                        if (!silentDownload) Console.WriteLine("Downloading \"" + fileName + "\" from " + downloadList[0][0]);
                                    }
                                    string dlToDir =
                                        dlDirectory + "/" + fileName.Substring(0, fileName.Length - GetFileName(fileName).Length);
                                    string dlAsName = fileName.Substring(fileName.Length - GetFileName(fileName).Length);
                                    DownloadFile(downloadList[0][0], dlToDir, dlAsName);
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleColor _c = Console.ForegroundColor;
                                Base.Error(e.Message);
                                Base.Error(fullFileName + "\nFrom " + downloadList[0][0]);
                                newFiles["failed"]++;
                                preProcent = -1;
                                //Thread.Sleep(1000);
                            }
                            downloadList.RemoveAt(0);
                        }
                        else
                        {
                            Console.WriteLine("Downloads are complete.");
                            return newFiles;
                        }
                    }
                    if (!silentDownload && procent != -1 && preProcent != procent)
                    {
                        BytePreference bp = bytePreference;

                        if (bp == BytePreference.Auto)
                        {
                            if (totalBytes < 1000)
                                bp = BytePreference.Bytes;
                            else if (totalBytes < 1000000)
                                bp = BytePreference.KiloBytes;
                            else if (totalBytes < 1000000000)
                                bp = BytePreference.MegaBytes;
                            else if (totalBytes >= 1000000000)
                                bp = BytePreference.GigaBytes;
                        }

                        if (bp == BytePreference.Bytes)
                            Console.Write(procent + "% | " + curbytes + " B/" + totalBytes + " B ");
                        else if (bp == BytePreference.KiloBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000.0, 2) + " KB/" + Math.Round(totalBytes / 1000.0, 2) + " KB ");
                        else if (bp == BytePreference.MegaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000.0, 2) + " MB/" + Math.Round(totalBytes / 1000000.0, 2) + " MB ");
                        else if (bp == BytePreference.GigaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000000.0, 2) + " GB/" + Math.Round(totalBytes / 1000000000.0, 2) + " GB ");
                        Console.WriteLine();
                        Console.Write("[");
                        for (int i = 0; i < 100; i += 2)
                        {
                            if (i < procent)
                            {
                                Base.WriteInColor("=", ConsoleColor.Green);
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        Console.Write("]");
                        Console.CursorTop -= 1;
                        Console.CursorLeft = 0;
                        preProcent = procent;
                    }
                }
            }
            #region StartDownload() overloads
            /// <summary>
            /// Start Download from downloadList
            /// </summary>
            /// <param name="bytePreference">Set how you want to show bytes, like Bytes, KiloBytes, MegaBytes and GigaBytes</param>
            public static Dictionary<string, int> StartDownload(BytePreference bytePreference)
            {
                DL.bytePreference = bytePreference;
                return StartDownload();
            }
            /// <summary>
            /// Start Download from URL
            /// </summary>
            /// <param name="url">The URL to download from</param>
            public static Dictionary<string, int> StartDownload(string url)
            {
                SetDlList(url);
                return StartDownload();
            }
            /// <summary>
            /// Start Download from an array of urls
            /// </summary>
            /// <param name="urls">The URL array to download from</param>
            public static Dictionary<string, int> StartDownload(string[] urls)
            {
                SetDlList(urls);
                return StartDownload();
            }
            #endregion

            private static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                procent = e.ProgressPercentage;
                curbytes = e.BytesReceived;
                totalBytes = e.TotalBytesToReceive;
            }

            private static void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
            {
                if (!silentDownload)
                {
                    Console.WriteLine("Download is completed");
                    Console.WriteLine(e.Result);
                }
            }
        }
    }

    // File manager
    namespace FileMangement
    {
        class FileManage
        {
            public static void CreateDirectory(string directory)
            {
                directory = directory.Replace("\\", "/");
                string[] _dirs = directory.Split('/');
                List<string> dirs = new List<string>(_dirs);
                string curDir = dirs[0] + "/";
                dirs.RemoveAt(0);
                while (!Directory.Exists(directory))
                {
                    if (Directory.Exists(curDir))
                    {
                        curDir += dirs[0] + "/";
                        dirs.RemoveAt(0);
                    }
                    else
                    {
                        Directory.CreateDirectory(curDir);
                    }
                }
            }
        }
    }
}