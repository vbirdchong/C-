using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Collections.Generic;

namespace DesktopWPFAppLowLevelKeyboardHook
{
    public class LowLevelKeyboardListener
    {
        public struct BarcodeInfo
        {
            public string strBarcode;
            public bool isValid;
            public DateTime time;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private List<char> _barcode = new List<char>(100);
        private BarcodeInfo _barCodeInfo = new BarcodeInfo();
        private List<int> _keyMap = new List<int>(11);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32", EntryPoint = "GetKeyboardState")]
        private static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32", EntryPoint = "ToAscii")]
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeyState, ref uint lpChar, int uFlags);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        //public event EventHandler<KeyPressedArgs> OnKeyPressed;

        // 条形码定义委托和事件
        public delegate void BarCodeDelegate(BarcodeInfo barCode);
        public event BarCodeDelegate BarCodeEvent;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public LowLevelKeyboardListener()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            InitKeyMap();
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// 键盘输入的回调函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                // 这里获取到的vkCode 其实就是对应ASCII中字符的码值
                // 如数字 0-9， 对应的码值就是 48-57
                int vkCode = Marshal.ReadInt32(lParam);

                // 只检查我们需要的字符：数字和回车
                if (_keyMap.Exists(a => a == vkCode))
                {
                    // 两个字符输入间隔>100ms则清空数据,扫描枪的输入很快
                    // 由于添加了>100ms的判断后，总是会将条形码的第一个值给丢掉，但不判断则读取数据正常，所以这里直接改成false
                    if (false /*DateTime.Now.Subtract(_barCodeInfo.time).TotalMilliseconds > 800*/)
                    {
                        _barcode.Clear();
                    }
                    else
                    {
                        if (KeyInterop.KeyFromVirtualKey(vkCode) == Key.Return && _barcode.Count > 0)
                        {
                            // 如果是return则已经输入完毕
                            _barCodeInfo.strBarcode = new String(_barcode.ToArray());
                            _barCodeInfo.isValid = true;
                            _barcode.Clear();
                        }
                        else
                        {
                            // 否则存到数组里
                            _barcode.Add((char)vkCode);
                        }
                    }
                }

                _barCodeInfo.time = DateTime.Now;
                if (BarCodeEvent != null && _barCodeInfo.isValid)
                {
                    // 触发事件
                    BarCodeEvent(_barCodeInfo);
                    _barCodeInfo.isValid = false;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void InitKeyMap()
        {
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D0));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D1));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D2));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D3));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D4));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D5));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D6));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D7));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D8));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.D9));
            _keyMap.Add(KeyInterop.VirtualKeyFromKey(Key.Return));
        }
    }
}
