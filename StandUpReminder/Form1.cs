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
    using System.Runtime.InteropServices;
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom;}

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);
        private IntPtr desktopHandle; //Window handle for the desktop    
        private IntPtr shellHandle; //Window handle for the shell    
        const int TIMER_MINUTES = 60;
        const int WARNING_SECONDS = 600;
        const int AUTOLOCK_SECONDS = 10;
        DateTime lastLoginDate = DateTime.Now;
        bool allowExit = false;
        Color originalBackColor;

        public Form1()
        {
            InitializeComponent();
        }

        private bool IsThereFullScreen()
        {
            //Get the handles for the desktop and shell now.    
            desktopHandle = GetDesktopWindow();
            shellHandle = GetShellWindow();
            //Detect if the current app is running in full screen    
            bool runningFullScreen = false;
            RECT appBounds;
            Rectangle screenBounds;
            IntPtr hWnd;
            //get the dimensions of the active window    
            hWnd = GetForegroundWindow();
            if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
            {
                //Check we haven't picked up the desktop or the shell        
                if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                {
                    GetWindowRect(hWnd, out appBounds);
                    //determine if window is fullscreen            
                    screenBounds = Screen.FromHandle(hWnd).Bounds;
                    if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width)
                    {
                        runningFullScreen = true;
                    }
                }
            }
            return runningFullScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - SystemInformation.VerticalScrollBarWidth, workingArea.Bottom - Size.Height - SystemInformation.HorizontalScrollBarHeight);

            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            originalBackColor = progressBar1.BackColor;

            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            String value = (string)key.GetValue("StandUpReminder");
            if (value == "dummy")
            {
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
            TimeSpan span = lastLoginDate.AddMinutes(TIMER_MINUTES).Subtract(DateTime.Now);
            TimeSpan showSpan = span.Subtract(TimeSpan.FromMinutes(10));
            int totalSeconds = (int)span.TotalSeconds;
            progressBar1.Value = totalSeconds;

            if (totalSeconds <= 0)
            {
                LockWorkStation();
                timer1.Enabled = false;
                return;
            }

            String prefix = "";

            if (totalSeconds > WARNING_SECONDS)
            {
                prefix = "";
            }
            else if (totalSeconds > AUTOLOCK_SECONDS)
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
                if (IsThereFullScreen()) Activate();
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
