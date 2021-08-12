using ShowUIApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ShowUI
{
    public class AgentSupport
    {
        public static bool EqualsUpToSeconds(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day &&
                   dt1.Hour == dt2.Hour && dt1.Minute == dt2.Minute && dt1.Second == dt2.Second;
        }
        public void SupportAgentDp()
        {
            try
            {
                var tool = Directory.GetFiles(@"F:\lsy\Test\DownloadConfig\AutoDL\AgentTool", "AgentDP.exx", SearchOption.TopDirectoryOnly).ToList();
                if (tool.Count <= 0)
                {
                    return;
                }
                FileInfo fInfo = new FileInfo(tool.First());
                if (!Directory.Exists(@"D:\AutoDL\AgentTool"))
                {
                    Directory.CreateDirectory(@"D:\AutoDL\AgentTool");
                }
                var toolLocal = Directory.GetFiles(@"D:\AutoDL\AgentTool", "AgentDP.exe", SearchOption.TopDirectoryOnly).ToList();
                //if (toolLocal.Count > 0)
                //{
                //    FileInfo fInfoTool = new FileInfo(toolLocal.First());
                //    if (EqualsUpToSeconds(fInfoTool.LastWriteTime, fInfo.LastWriteTime))
                //    {
                //        return;
                //    }
                //}
                File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\AgentTool\AgentDP.exx", @"D:\AutoDL\AgentTool\AgentDP.exe", true);
                Process.Start(@"D:\AutoDL\AgentTool\AgentDP.exe");
            }
            catch 
            {

                
            }
            
        }


        public void ClearFolder()
        {
            try
            {
                if (File.Exists(@"C:\SFISKernel.exe"))
                {
                    File.Delete(@"C:\SFISKernel.exe");
                }
                if (File.Exists(@"C:\kernel.ini"))
                {
                    File.Delete(@"C:\kernel.ini");
                }
                if (File.Exists(@"C:\SFISKerneS.exe"))
                {
                    File.Delete(@"C:\SFISKerneS.exe");
                }
            }
            catch { }



        }
        public void DownLoadFtp(string localPath, string ftpPath)
        {
            try
            {


                string user = "argent";
                string pass = "123";
                using (WebClient req = new WebClient())
                {
                    req.Credentials = new NetworkCredential(user, pass);
                    
                    byte[] fileData = req.DownloadData(ftpPath);
                    using (FileStream file = File.Create(localPath))
                    {
                        file.Write(fileData, 0, fileData.Length);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {


            }
        }

        public void StartArgent()
        {
            try
            {
                //var lstArgent = Process.GetProcessesByName("SFISKernel").ToList();
                //var lstArgentS = Process.GetProcessesByName("SFISKerneS").ToList();
                //var lstSumArgent = lstArgentS.Union(lstArgent);
                //if (!File.Exists(@"C:\UpdateArgent.txt"))
                //{
                //    File.Create(@"C:\UpdateArgent.txt");
                //}


                // ClearFolder();
                //  DownLoadFtp(@"C:\SFISKernel.exe", "ftp://10.224.81.62/SFISKernel.exe");
                //  DownLoadFtp(@"C:\kernel.ini", "ftp://10.224.81.62/kernel.ini");
                var pro = Process.GetProcessesByName("SFISKernel").ToList();
                if (pro.Count < 1)
                {
                    ProcessStartInfo pStart = new ProcessStartInfo();
                    pStart.WorkingDirectory = @"C:\";
                    pStart.FileName = "SFISKernel.exe";
                    Process.Start(pStart);
                }
                   
                
            }
            catch (Exception)
            {


            }
        }
    }
}
