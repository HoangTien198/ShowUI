using System.IO;

namespace ShowUI
{
    public class DownLoadToolsBoxHelper
    {
        public void downLoadApp()
        {
            FileInfo[] files = new DirectoryInfo(@"F:\lsy\Test\DownloadConfig\AutoDL\Toolsbox").GetFiles();
            if (!Directory.Exists(@"D:\AutoDL\Toolsbox"))
            {
                Directory.CreateDirectory(@"D:\AutoDL\Toolsbox");
            }

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i].Name;

                if (files[i].Extension.ToUpper().Contains(".EX"))
                {
                    fileName = files[i].Name.Substring(0, files[i].Name.LastIndexOf(files[i].Extension)) + ".exe";
                }
                files[i].CopyTo(@"D:\AutoDL\Toolsbox" + "\\" + fileName, true);
            }
        }
    }
}