using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ShowUIApp
{
    internal class IniFile
    {
        private string fileName;

        /// <summary>
        /// Creates a new <see cref="IniFile"/> instance.
        /// </summary>
        /// <param name="fileName">Name of the INI file.</param>
        public IniFile(string fileName)
        {
            this.fileName = fileName;
        }

        // native methods
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
          string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileSection(string section, IntPtr lpReturnedString,
          int nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(string ApplicationName, string KeyName, string StrValue, string FileName);

        /// <summary>
        /// Reads a string value from the INI file.
        /// </summary>
        /// <param name="section">Section to read.</param>
        /// <param name="key">Key to read.</param>
        public string ReadString(string section, string key)
        {
            const int bufferSize = 255;
            StringBuilder temp = new StringBuilder(bufferSize);
            GetPrivateProfileString(section, key, "", temp, bufferSize, fileName);
            return temp.ToString();
        }

        public static string ReadIniFile(string section, string key, string _default, string fileName, int ibufferSize = 255)
        {
            int bufferSize = ibufferSize;
            StringBuilder temp = new StringBuilder(bufferSize);
            GetPrivateProfileString(section, key, "", temp, bufferSize, fileName);
            if (temp.Length > 0)
            {
                return temp.ToString();
            }
            return _default;
        }

        public static void WriteValue(string SectionName, string KeyName, string KeyValue, string FileName)
        {
            WritePrivateProfileString(SectionName, KeyName, KeyValue, FileName);
        }

        /// <summary>
        /// Reads a whole section of the INI file.
        /// </summary>
        /// <param name="section">Section to read.</param>
        public string[] ReadSection(string section)
        {
            const int bufferSize = 2048;

            StringBuilder returnedString = new StringBuilder();

            IntPtr pReturnedString = Marshal.AllocCoTaskMem(bufferSize);
            try
            {
                int bytesReturned = GetPrivateProfileSection(section, pReturnedString, bufferSize, fileName);

                //bytesReturned -1 to remove trailing \0
                for (int i = 0; i < bytesReturned - 1; i++)
                    returnedString.Append((char)Marshal.ReadByte(new IntPtr((uint)pReturnedString + (uint)i)));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pReturnedString);
            }

            string sectionData = returnedString.ToString();
            return sectionData.Split('\0');
        }
    }
}