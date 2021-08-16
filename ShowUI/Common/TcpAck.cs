using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace ShowUI.Common
{
    public class TcpAck
    {
        public void SetTcpAckByDUT()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                List<NetworkInterface> Duts = interfaces.Where(x => x.Name.ToUpper().Contains("DUT")).ToList();
                foreach (var Dut in Duts)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + Dut.Id, true);
                    if (key != null)
                    {
                        key.SetValue("TcpAckFrequency", "1", RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
            }
        }
    }
}