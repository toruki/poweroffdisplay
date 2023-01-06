using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerOffDisplay
{
    public partial class LockDetect : Form
    {
        [DllImport("WtsApi32.dll")]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);
        [DllImport("WtsApi32.dll")]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int PostMessage(int hwnd, int msg, int wParam, int lParam);


        const int NOTIFY_FOR_THIS_SESSION = 0;

        const int WM_WTSSESSION_CHANGE = 0x2b1;

        const int WTS_SESSION_LOCK = 0x7;
        const int WTS_SESSION_UNLOCK = 0x8;
        const int WTS_SESSION_REMOTE_CONTROL = 0x9;

        const int HWND_BROADCAST = 0xFFFF;
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MONITORPOWER = 0xF170;

        const int DISPLAY_ON = -1;
        const int DISPLAY_OFF = 2;

        private volatile bool currentLockState = false;


        public LockDetect()
        {
            // 画面状態の通知に必要
            WTSRegisterSessionNotification(this.Handle, NOTIFY_FOR_THIS_SESSION);

            Task.Run(MonitorLock);
        }

        /// <summary>
        /// ロック状態の監視
        /// 
        /// 定期的に画面ロックしているかを確認する
        /// ・ロックしていないときは何もしない
        /// ・ロックしていた場合、ｎ秒後にロックしていたら電源オフ
        /// </summary>
        private void MonitorLock()
        {
            int lockCount = 0;
            while (true)
            {
                if (currentLockState == false)
                {
                    lockCount = 0;
                }
                else if (++lockCount > 10)
                {
                    DisplayPower(false);
                    lockCount = -30;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        protected override void WndProc(ref Message m)
        {
            // 画面状態が変更された
            if (m.Msg == WM_WTSSESSION_CHANGE)
            {
                int value = m.WParam.ToInt32();

                switch (value)
                {
                    case WTS_SESSION_LOCK:
                        Console.WriteLine("PCがロックされました");
                        currentLockState = true;
                        break;

                    case WTS_SESSION_UNLOCK:
                        Console.WriteLine("PCのロックが解除されました");
                        currentLockState = false;
                        break;

                    case WTS_SESSION_REMOTE_CONTROL:
                        Console.WriteLine("PCがRDP制御されました");
                        currentLockState = false;
                        break;
                    default:
                        break;
                }
            }

            base.WndProc(ref m);
        }

        public static void DisplayPower(bool on)
        {
            PostMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, on ? DISPLAY_ON : DISPLAY_OFF);
        }
    }
}
