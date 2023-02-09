using System;
using System.Diagnostics;
using System.IO;
using System.Net;


namespace pWorkTimerUpdateRepair
{
    class Program
    {
        static string programName = "pWorkTimer";
        static string dll = programName + ".dll";
        static string exe = programName + ".exe";
        static string execonfig = programName + ".exe.config";
        static string exemanifest = programName + ".exe.manifest";
        static string pdb = programName + ".pdb";
        static string json = programName + ".runtimeconfig.json";
        static string rtjson = programName + ".runtimeconfig.json";
        static string despjson = programName + ".deps.json";

        static string[] files = new string[] { dll, exe, execonfig, exemanifest, pdb, json, rtjson, despjson };

        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            string siteData = client.DownloadString("http://prestonpeek.weebly.com/downloads.html");
            string fileInfo = siteData.Split('~')[4];
            string fileName = fileInfo.Split(',')[0];
            string fileId = fileInfo.Split(',')[1];
            string downloadLink = "https://drive.google.com/uc?export=download&id=" + fileId;
            client.DownloadFile(downloadLink, fileName + ".zip");
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(fileName + ".zip", "./");
            File.Delete(fileName + ".zip");

            Process.Start("pWorkTimer.exe");
        }

    }
}
