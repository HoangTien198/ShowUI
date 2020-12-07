using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    }
}
