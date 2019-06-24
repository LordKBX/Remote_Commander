using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Extention_WinKeyboard
{
    public class Keyboard
    {
        [DllImport("user32.dll")] static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public const uint VK_NULL = 0x00;
        public const uint VK_LCONTROL = 0xA2;
        public const uint VK_LMAJ = 0xA0;
        public const uint VK_LALT = 0x12;
        public const uint VK_LWIN = 0x5B;
        public const uint VK_INSERT = 0x2D;

        public const uint VK_BACK = 0x08;
        public const uint VK_TAB = 0x09;
        public const uint VK_RETURN = 0x0D;
        public const uint VK_ESCAPE = 0x1B;
        public const uint VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        public const uint VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        public const uint VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to prev track

        public static void Send(char a, bool alt = false, bool ctrl = false, bool maj = false, bool win = false)
        {
            Console.WriteLine("lang = " + Thread.CurrentThread.CurrentCulture.Name);
            var keyboard = new KeyboardPointer(Thread.CurrentThread.CurrentCulture);
            byte key = 0;
            keyboard.GetKey(a, out key);
            Debug.WriteLine("IN PLUGIN, char = " + a);
            Debug.WriteLine("IN PLUGIN, char in byte = " + key);
            Debug.WriteLine("alt = " + alt);
            Debug.WriteLine("ctrl = " + ctrl);
            Debug.WriteLine("maj = " + maj);
            Debug.WriteLine("win = " + win);
            
            if(alt == true) { KeyDown((byte)VK_LALT); }
            if(ctrl == true) { KeyDown((byte)VK_LCONTROL); }
            if(maj == true) { KeyDown((byte)VK_LMAJ); }
            if(win == true) { KeyDown((byte)VK_LWIN); }
            if (a != 0)
            {
                KeyDown(key);
                KeyUp(key);
            }
            if (alt == true) { KeyUp((byte)VK_LALT); }
            if (ctrl == true) { KeyUp((byte)VK_LCONTROL); }
            if (maj == true) { KeyUp((byte)VK_LMAJ); }
            if (win == true) { KeyUp((byte)VK_LWIN); }
        }
        public static void SendSpe(uint a, bool alt = false, bool ctrl = false, bool maj = false, bool win = false)
        {
            if (alt == true) { KeyDown((byte)VK_LALT); }
            if (ctrl == true) { KeyDown((byte)VK_LCONTROL); }
            if (maj == true) { KeyDown((byte)VK_LMAJ); }
            if (win == true) { KeyDown((byte)VK_LWIN); }
            if (a != 0)
            {
                KeyDown((byte)a);
                KeyUp((byte)a);
            }
            if (alt == true) { KeyUp((byte)VK_LALT); }
            if (ctrl == true) { KeyUp((byte)VK_LCONTROL); }
            if (maj == true) { KeyUp((byte)VK_LMAJ); }
            if (win == true) { KeyUp((byte)VK_LWIN); }
        }
        private static void KeyDown(byte key)
        {
            keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        private static void KeyUp(byte key)
        {
            keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }
    
    public class KeyboardPointer : IDisposable
    {
        [DllImport("user32.dll")] static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);
        [DllImport("user32.dll")] static extern bool UnloadKeyboardLayout(IntPtr hkl);
        [DllImport("user32.dll")] static extern short VkKeyScanExA(char ch, IntPtr dwhkl);
        private readonly IntPtr pointer;
        public KeyboardPointer(int klid)
        {
            pointer = LoadKeyboardLayout(klid.ToString("X8"), 1);
        }
        public KeyboardPointer(System.Globalization.CultureInfo culture)
          : this(culture.KeyboardLayoutId) { }
        public void Dispose()
        {
            UnloadKeyboardLayout(pointer);
            GC.SuppressFinalize(this);
        }
        ~KeyboardPointer()
        {
            UnloadKeyboardLayout(pointer);
        }
        // Converting to System.Windows.Forms.Key here, but
        // some other enumerations for similar tasks have the same
        // one-to-one mapping to the underlying Windows API values
        public bool GetKey(char character, out byte key)
        {
            short keyNumber = VkKeyScanExA(character, pointer);
            if (keyNumber == -1)
            {
                key = 0;
                return false;
            }
            key = (byte)(((keyNumber & 0xFF00) << 8) | (keyNumber & 0xFF));
            return true;
        }
        public bool GetKeyShort(char character, out short key)
        {
            short keyNumber = VkKeyScanExA(character, pointer);
            if (keyNumber == -1)
            {
                key = 0;
                return false;
            }
            key = keyNumber;
            return true;
        }
    }
}
