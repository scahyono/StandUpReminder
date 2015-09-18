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
        Color originalBackColor;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - SystemInformation.VerticalScrollBarWidth, workingArea.Bottom - Size.Height - SystemInformation.HorizontalScrollBarHeight);

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            originalBackColor = progressBar1.BackColor;

            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            String value = (string)key.GetValue("StandUpReminder");
            if (value == "dummy") {
                key.SetValue("StandUpReminder", System.Reflection.Assembly.GetEntryAssembly().Location, RegistryValueKind.String);
            }
            key.Close();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan span = lastLoginDate.AddMinutes(60).Subtract(DateTime.Now);
            TimeSpan showSpan = span.Subtract(TimeSpan.FromMinutes(10));
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
                prefix = "";
            }
            else if (totalSeconds > 60)
            {
                progressBar1.BackColor = Color.Yellow;
                prefix = "Please, lock Windows now and walk away! ";
                Show();
                showSpan = span;
            } 
            else
            {
                progressBar1.BackColor = Color.Red;
                prefix = "Automatic lock imminent! ";
                Show();
                showSpan = span;
            }
            string timeLeft = showSpan.Minutes.ToString("00") + ":" + showSpan.Seconds.ToString("00");
            label1.Text = prefix + timeLeft + " minutes left";
            notifyIcon1.Text = "Stand Up Reminder " + timeLeft;
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
                progressBar1.ForeColor = Color.Green;
                progressBar1.BackColor = originalBackColor;
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
