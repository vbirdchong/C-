using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace IdConfirmApp
{
    class KeyboardHook
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;

        static int hKeyboardHook = 0;
        public const int WH_KEYBOARD_LL = 13;
        HookProc KeyboardHookProcedure;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr IParam);

        public struct BarCodes
        {
            public int VirtKey;//虚拟吗
            public int ScanCode;//扫描码
            public string KeyName;//键名
            public uint Ascll;//Ascll
            public char Chr;//字符
            public string OriginalChrs; //原始 字符
            public string OriginalAsciis;//原始 ASCII


            public string OriginalBarCode; //原始数据条码

            public string BarCode;//条码信息 保存最终的条码
            public bool IsValid;//条码是否有效
            public DateTime Time;//扫描时间,
        }

        private struct EventMsg
        {
            public int message;
            public int paramL;
            public int paramH;
            public int Time;
            public int hwnd;
        }

        BarCodes barCode = new BarCodes();
        StringBuilder sbBarCode = new StringBuilder();

        //[StructLayout(LayoutKind.Sequential)]
        //public class KeyboardHookStruct
        //{
        //    public int vkCode;
        //    public int scanCode;
        //    public int flags;
        //    public int time;
        //    public int dwExtraInfo;
        //}

        //使用此功能，安装了一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //调用此函数卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);


        //使用此功能，通过信息钩子继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        // 取得当前线程编号（线程钩子需要用到）
        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        private static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);

        //使用WINDOWS API函数代替获取当前实例的函数,防止钩子失效
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void Start()
        {
            // 安装键盘钩子
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName), 0);
                //hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                //************************************
                //键盘线程钩子
                //SetWindowsHookEx( 2,KeyboardHookProcedure, IntPtr.Zero, GetCurrentThreadId());//指定要监听的线程idGetCurrentThreadId(),
                //键盘全局钩子,需要引用空间(using System.Reflection;)
                //SetWindowsHookEx( 13,MouseHookProcedure,Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),0);
                //
                //关于SetWindowsHookEx (int idHook, HookProc lpfn, IntPtr hInstance, int threadId)函数将钩子加入到钩子链表中，说明一下四个参数：
                //idHook 钩子类型，即确定钩子监听何种消息，上面的代码中设为2，即监听键盘消息并且是线程钩子，如果是全局钩子监听键盘消息应设为13，
                //线程钩子监听鼠标消息设为7，全局钩子监听鼠标消息设为14。lpfn 钩子子程的地址指针。如果dwThreadId参数为0 或是一个由别的进程创建的
                //线程的标识，lpfn必须指向DLL中的钩子子程。 除此以外，lpfn可以指向当前进程的一段钩子子程代码。钩子函数的入口地址，当钩子钩到任何
                //消息后便调用这个函数。hInstance应用程序实例的句柄。标识包含lpfn所指的子程的DLL。如果threadId 标识当前进程创建的一个线程，而且子
                //程代码位于当前进程，hInstance必须为NULL。可以很简单的设定其为本应用程序的实例句柄。threaded 与安装的钩子子程相关联的线程的标识符
                //如果为0，钩子子程与所有的线程关联，即为全局钩子
                //************************************
                //如果SetWindowsHookEx失败
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败");
                }
                else
                {
                    Console.WriteLine("安装成功");
                }
            }
        }

        public void Stop()
        {
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }

            if (!(retKeyboard)) throw new Exception("卸载钩子失败！");
        }

        //ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符
        //[DllImport("user32")]
        //public static extern int ToAscii(int uVirtKey, //[in] 指定虚拟关键代码进行翻译。
        //                                 int uScanCode, // [in] 指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压）
        //                                 byte[] lpbKeyState, // [in] 指针，以256字节数组，包含当前键盘的状态。每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。在低比特，如果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。在切换状态的NUM个锁和滚动锁定键被忽略。
        //                                 byte[] lpwTransKey, // [out] 指针的缓冲区收到翻译字符或字符。
        //                                 int fuState); // [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise.

        //获取按键的状态
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);


        [DllImport("user32", EntryPoint = "ToAscii")]
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private const int WM_KEYDOWN = 0x100;//KEYDOWN
        private const int WM_KEYUP = 0x101;//KEYUP
        private const int WM_SYSKEYDOWN = 0x104;//SYSKEYDOWN
        private const int WM_SYSKEYUP = 0x105;//SYSKEYUP

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));
            if (wParam == WM_KEYDOWN)
            {
                barCode.VirtKey = msg.message & 0xff;//虚拟吗
                barCode.ScanCode = msg.paramL & 0xff;//扫描码
                StringBuilder strKeyName = new StringBuilder(225);
                if (GetKeyNameText(barCode.ScanCode * 65536, strKeyName, 255) > 0)
                {
                    barCode.KeyName = strKeyName.ToString().Trim(new char[] { ' ', '\0' });
                }
                else
                {
                    barCode.KeyName = "";
                }
            }

            byte[] kbArray = new byte[256];
            uint uKey = 0;
            GetKeyboardState(kbArray);


            if (ToAscii(barCode.VirtKey, barCode.ScanCode, kbArray, ref uKey, 0))
            {
                barCode.Ascll = uKey;
                barCode.Chr = Convert.ToChar(uKey);
            }

            TimeSpan ts = DateTime.Now.Subtract(barCode.Time);

            Console.WriteLine("time1:{0}",ts);

            if (ts.TotalMilliseconds > 50)
            {
                //时间戳，大于50 毫秒表示手动输入
                //strBarCode = barCode.Chr.ToString();
                sbBarCode.Remove(0, sbBarCode.Length);
                sbBarCode.Append(barCode.Chr.ToString());
                barCode.OriginalChrs = " " + Convert.ToString(barCode.Chr);
                barCode.OriginalAsciis = " " + Convert.ToString(barCode.Ascll);
                barCode.OriginalBarCode = Convert.ToString(barCode.Chr);
            }
            else
            {
                sbBarCode.Append(barCode.Chr.ToString());
                if ((msg.message & 0xff) == 13 && sbBarCode.Length > 3)
                {//回车
                    //barCode.BarCode = strBarCode;
                    barCode.BarCode = sbBarCode.ToString();// barCode.OriginalBarCode;
                    barCode.IsValid = true;
                    sbBarCode.Remove(0, sbBarCode.Length);
                }
                //strBarCode += barCode.Chr.ToString();
            }
            barCode.Time = DateTime.Now;

            // 下面的代码暂时不用
            //// 侦听键盘事件
            //if ((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            //{
            //    if (KeyDownEvent != null)
            //    {
            //        Console.WriteLine("键盘按下");
            //    }
            //    KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
            //    // raise KeyDown
            //    if (KeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
            //    {
            //        Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
            //        KeyEventArgs e = new KeyEventArgs(keyData);
            //        KeyDownEvent(this, e);
            //    }
     
            //    //键盘按下
            //    if (KeyPressEvent != null && wParam == WM_KEYDOWN)
            //    {
            //        byte[] keyState = new byte[256];
            //        GetKeyboardState(keyState);
     
            //        byte[] inBuffer = new byte[2];
            //        if (ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
            //        {
            //            KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
            //            KeyPressEvent(this, e);
            //        }
            //    }
     
            //    // 键盘抬起
            //    if (KeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
            //    {
            //        Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
            //        KeyEventArgs e = new KeyEventArgs(keyData);
            //        KeyUpEvent(this, e);
            //    }
     
            //}
            //如果返回1，则结束消息，这个消息到此为止，不再传递。
            //如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        ~KeyboardHook()
        {
            Stop();
        }

        public void hook_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.KeyValue);
            //判断按下的键（Alt + A）
            if (e.KeyValue == (int)Keys.A /*&& (int)Control.ModifierKeys == (int)Keys.Alt*/)
            {
                //System.Windows.Forms.MessageBox.Show("按下了指定快捷键组合");
                //Console.WriteLine();
            }
        }

        public String GetBarcode()
        {
            Console.WriteLine(sbBarCode.ToString());
            return sbBarCode.ToString();
        }
    }
}
