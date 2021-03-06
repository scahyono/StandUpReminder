﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandUpReminder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string procName = Process.GetCurrentProcess().ProcessName.Split('.')[0];

            Process[] processes = Process.GetProcesses();
            int count = 0;
            foreach (Process process in processes)
            {
                if (process.ProcessName.StartsWith(procName))
                {
                    count++;
                }
            }

            if (count > 1)
            {
                MessageBox.Show(procName + " already running");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
