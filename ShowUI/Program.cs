using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using ShowUI;

namespace ShowUIApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Form1 frm;
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new showUI());
           // Application.Run(new frmStationWarning());
            //Application.Run(new Form1());
            //frm = new Form1();
            //SingleInstanceApplication.Run(frm, NewInstanceHandler);

        }
        public static void NewInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            frm.Show();
            frm.Activate();

        }
        public class SingleInstanceApplication : WindowsFormsApplicationBase
        {
            private SingleInstanceApplication()
            {
                base.IsSingleInstance = true;
            }
            public static void Run(Form f, StartupNextInstanceEventHandler startupHandle)
            {
                SingleInstanceApplication App = new SingleInstanceApplication();
                App.MainForm = f;
                App.StartupNextInstance += startupHandle;
                App.Run(Environment.GetCommandLineArgs());
            }
        }

    }
}