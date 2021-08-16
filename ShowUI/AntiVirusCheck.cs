using System;
using System.Diagnostics;
using System.IO;

namespace ShowUI
{
    public class AntiVirusCheck
    {
        public static void CreateFolder()
        {
            try
            {
                if (!Directory.Exists(".//Antivirut"))
                {
                    Directory.CreateDirectory(".//Antivirut");
                }
                if (!File.Exists(".//Antivirut//Antivirut.exe"))
                {
                    File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\Antivirut\Antivirut.exx", ".//Antivirut//Antivirut.exe", true);
                    //File.Delete(".//IQTESTTIME//LocalDetailLog.ini");
                }
                Process[] pro = Process.GetProcessesByName("Antivirut");
                foreach (var item in pro)
                {
                    item.Kill();
                }
                Process.Start(".//Antivirut//Antivirut.exe");
            }
            catch (Exception)
            {
            }

            //File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTESTTIME\LocalDetailLog.ini", ".//IQTESTTIME//LocalDetailLog.ini", false);
            //File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTESTTIME\IQCheckTimeItem.ini", ".//IQTESTTIME//IQCheckTimeItem.ini", false);
        }
    }
}