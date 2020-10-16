
using Microsoft.Win32;
using ShowUIApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ShowUI.Helper
{
    public class FakeShowUIHelper
    {
        public FakeModel FakeUI(string model,string station)
        {
           
            string stringFake = IniFile.ReadIniFile(model, station, "0", ".\\FShowUIConfig.txt");
            if(!File.Exists(".\\FShowUIConfig.txt"))
            {
                File.Create(".\\FShowUIConfig.txt");
            }
            if (stringFake.Length < 4)
            {
                IniFile.WriteValue(model, station, "false;3;-1_0;3;-0.5_0.5;99.14;0_0.5", ".\\FShowUIConfig.txt");
            }
            string[] resultFake = stringFake.Split(';');
            if (stringFake.Length > 1 && resultFake[0].Contains("true"))
            {
				IniFile.WriteValue(model, station, "false;3;-1_0;3;-0.5_0.5;99.14;0_0.5", ".\\FShowUIConfig.txt");
				return  new FakeModel()
                {
                    fakeTRR = double.Parse(resultFake[1]),
                    spaceRandTRR1 = double.Parse(resultFake[2].Split('_')[0]),
                    spaceRandTRR2 = double.Parse(resultFake[2].Split('_')[1]),
                    fakeSRR = double.Parse(resultFake[3]),
                    spaceRandSRR1 = double.Parse(resultFake[4].Split('_')[0]),
                    spaceRandSRR2 = double.Parse(resultFake[4].Split('_')[1]),
                    fakeTYR = double.Parse(resultFake[5]),
                    spaceRandTYR1 = double.Parse(resultFake[6].Split('_')[0]),
                    spaceRandTYR2 = double.Parse(resultFake[6].Split('_')[1]),
                    nofake = false
                };
                
            
            }
            else
            {

            return null;
            }
            

            

        }

        public double RandomResult(double? valueFix, double? spacelow,double? spacehight)
        {
            Random rd = new Random();
            return valueFix.Value + rd.NextDouble() * (spacehight.Value - spacelow.Value) + spacelow.Value;
        }
        public double RandomTwoValue(double spacelow, double spacehight)
        {
            Random rd = new Random();
            return  rd.NextDouble() * (spacehight - spacelow) + spacelow;
        }
        public double ConvertToDouble(string data)
        {
            if (data.Contains("%"))
            {
                return double.Parse(data.Split('%')[0]);
            }
            return 0;
        }
      

    }
}
