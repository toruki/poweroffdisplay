using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;


namespace PowerOffDisplay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ContextMenuStrip menu = new();
        NotifyIcon notifyIcon = new();

        private LockDetect lockDetect = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            // 右クリックに出すコンテキストメニュー
            menu.Items.Add("&Exit", null, (obj, e) => { Shutdown(); });

            // タスクトレイ上のアイコン
            notifyIcon.Visible = true;
            notifyIcon.Icon = PowerOffDisplay.Properties.Resources.PowerOffDisplay;
            notifyIcon.Text = "スクリーンロック状態を検知してディスプレイの電源を切ります";
            notifyIcon.ContextMenuStrip = menu;

            // アイコンを押したときの処理
            notifyIcon.Click += (obj, e) =>
            {
                var mouseEvent = e as MouseEventArgs;
                if (mouseEvent is not null && mouseEvent.Button != MouseButtons.Right)
                {
                    LockScreen();
                }
            };

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            menu.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            base.OnExit(e);
        }

        private static void LockScreen()
        {
            Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }

    }
}
