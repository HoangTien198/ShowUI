using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShowUI
{
    public class IQtesttimehelper
    {
        public static void CreateFolder()
        {
            //create Iqtestime folder and copy ini file
            if (!Directory.Exists(".//IQTESTTIME"))
            {
                Directory.CreateDirectory(".//IQTESTTIME");
            }   
            if (File.Exists(".//IQTESTTIME//LocalDetailLog.ini"))
            {
                File.Delete(".//IQTESTTIME//LocalDetailLog.ini");
            }

            if (File.Exists(".//IQTESTTIME//IQCheckTimeItem.ini"))
            {
                File.Delete(".//IQTESTTIME//IQCheckTimeItem.ini");
            }

            File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTESTTIME\LocalDetailLog.ini", ".//IQTESTTIME//LocalDetailLog.ini", false);
            File.Copy(@"F:\lsy\Test\DownloadConfig\AutoDL\IQTESTTIME\IQCheckTimeItem.ini", ".//IQTESTTIME//IQCheckTimeItem.ini", false);

        }
    }
}
