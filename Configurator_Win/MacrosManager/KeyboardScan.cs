using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Configurator_Win.MacrosManager
{
    public partial class KeyboardScan : Form
    {
        private ActionEditor parent = null;

        public const uint VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        public const uint VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        public const uint VK_MEDIA_STOP = 0xB2;// code to STOP a song
        public const uint VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to prev track

        public const uint VK_LCONTROL = 0x11;
        public const uint VK_LMAJ = 0x10;
        public const uint VK_LALT = 0x12;
        public const uint VK_LWIN = 0x5B;
        public const uint VK_INSERT = 0x2D;

        public const uint VK_BACK = 0x08;
        public const uint VK_TAB = 0x09;
        public const uint VK_RETURN = 0x0D;
        public const uint VK_ESCAPE = 0x1B;

        public KeyboardScan(ActionEditor parent)
        {
            this.parent = parent;
            InitializeComponent();
            this.FormClosing += KeyboardScan_FormClosing;
        }

        private void KeyboardScan_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void KeyboardScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == VK_LCONTROL) { parent.SetKeyboardActionCtrl(true); }
            if (e.KeyValue == VK_LALT) { parent.SetKeyboardActionAlt(true); }
            if (e.KeyValue == VK_LMAJ) { parent.SetKeyboardActionMaj(true); }
            if (e.KeyValue == VK_LWIN) { parent.SetKeyboardActionWin(true); }
            Debug.WriteLine("Key Down: "+ e.KeyValue);
        }

        private void KeyboardScan_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyValue == VK_LCONTROL) { parent.SetKeyboardActionCtrl(false); return; }
            else if (e.KeyValue == VK_LALT) { parent.SetKeyboardActionAlt(false); return; }
            else if (e.KeyValue == VK_LMAJ) { parent.SetKeyboardActionMaj(false); return; }
            else if (e.KeyValue == VK_LWIN) { parent.SetKeyboardActionWin(false); return; }
            else if (e.KeyValue == VK_MEDIA_PLAY_PAUSE) { parent.SetKeyboardActionKey("MEDIA_PLAY_PAUSE"); }
            else if (e.KeyValue == VK_MEDIA_STOP) { parent.SetKeyboardActionKey("MEDIA_STOP"); }
            else if (e.KeyValue == VK_MEDIA_NEXT_TRACK) { parent.SetKeyboardActionKey("MEDIA_NEXT_TRACK"); }
            else if (e.KeyValue == VK_MEDIA_PREV_TRACK) { parent.SetKeyboardActionKey("MEDIA_PREV_TRACK"); }
            else if (e.KeyValue == VK_ESCAPE) { parent.SetKeyboardActionKey("ESCAPE"); }
            else if (e.KeyValue == VK_TAB) { parent.SetKeyboardActionKey("TAB"); }
            else if (e.KeyValue == VK_RETURN) { parent.SetKeyboardActionKey("ENTER"); }
            else if (e.KeyValue == VK_BACK) { parent.SetKeyboardActionKey("BACKSPACE"); }
            else {
                string rez = KeyCodeToUnicode(e.KeyCode);
                if (rez == "") { return; }
                parent.SetKeyboardActionKey(rez);
            }
            this.Close();
        }

        public string KeyCodeToUnicode(Keys key)
        {
            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

    }
    
}
