using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Extention_WinTrayIcon
{
    public class TrayIcon
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public void HideConsole() {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        public void ShowConsole() {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }
    }
}
