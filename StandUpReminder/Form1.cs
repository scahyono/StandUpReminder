using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandUpReminder
{
    public partial class Form1 : Form
    {

        DateTime lastLoginDate = DateTime.Now;
        bool allowExit = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan span = lastLoginDate.AddMinutes(60).Subtract(DateTime.Now);
            int totalSeconds = (int)span.TotalSeconds;
            progressBar1.Value = totalSeconds;

            if (totalSeconds <= 0) {
                Process.Start(@"C:\Windows\system32\rundll32.exe","user32.dll,LockWorkStation");
                timer1.Enabled = false;
                return;
            }

            String prefix = "";

            if (totalSeconds > 600)
            {
                progressBar1.ForeColor = Color.Green;
                prefix = "";
            }
            else if (totalSeconds > 60)
            {
                progressBar1.ForeColor = Color.Yellow;
                prefix = "Please, lock Windows now and walk away! ";
                Show();
            } 
            else
            {
                progressBar1.ForeColor = Color.Red;
                prefix = "Automatic lock imminent! ";
                Show();
            }
            string timeLeft = span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
            label1.Text = prefix + timeLeft + " minutes left";
            notifyIcon1.Text = "Stand Up Reminder " + timeLeft;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            Console.ReadLine();
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                timer1.Enabled = false;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.SessionLogon)
            {
                timer1.Enabled = true;
                lastLoginDate = DateTime.Now;
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            if (!allowExit) e.Cancel = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowExit = true;
            Application.Exit();
        }

        private void progressBar1_MouseLeave(object sender, EventArgs e)
        {
            Opacity = .5;
        }

        private void progressBar1_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1;
        }

    }
}
