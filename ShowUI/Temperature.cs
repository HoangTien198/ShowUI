using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace ShowUI
{
    public class Temperature
    {
        public double CurrentValue { get; set; }
        public string InstanceName { get; set; }

        public static List<Temperature> Temperatures
        {
            get
            {
                List<Temperature> result = new List<Temperature>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get())
                {
                    Double temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                    temperature = (temperature - 2732) / 10.0;
                    result.Add(new Temperature { CurrentValue = temperature, InstanceName = obj["InstanceName"].ToString() });
                }
                return result;
            }
        }
    }
}
