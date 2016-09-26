using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using DesktopWPFAppLowLevelKeyboardHook;
using System.IO;
using Mmmer.Queuer.Transfering.Dto;
using dll_vguang_app_csharp;
using System.Runtime.InteropServices;


namespace BarcodeMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public struct TicketInfo {
            public TicketDto baseInfo;
            public Utils.cBabyInfo babyInfo;
        }

        // 即插即用型全局键盘监听
        private LowLevelKeyboardListener _listener;
        private NotifyIcon notifyIcon;
        private TicketInfo babyCreateResult;

        // VG 扫码器
        private Vguang vgListern;
        private Vguang.DecodeCallBack vgDecodeCall;


        public MainWindow()
        {
            InitializeComponent();

            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("Open");
            open.Click += new EventHandler(Show);
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });
        }

        private void Show(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

// 使用全局键盘监听实现，对于即插即用型的条形扫描枪可以使用这种方法
#if false 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.BarCodeEvent += _listener_OnKeyPressed;
            _listener.HookKeyboard();
            if (ConfigHelper.GetConfigInfo() == false)
            {
                System.Windows.MessageBox.Show("错误!请检查配置文件中 Https.url/Authority 是否有进行设置?\n设置完毕后，请重新启动该应用程序！");
            }

            this.Hide();
            this.ShowInTaskbar = true;
        }

        void _listener_OnKeyPressed(LowLevelKeyboardListener.BarcodeInfo barCodeInfo)
        {
            // 条形码显示
            this.textBox_DisplayKeyboardInput.Text = barCodeInfo.strBarcode;
            // 本地调试需要注释掉
            StartMonitor(barCodeInfo.strBarcode);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

#else
        // VG的扫码器需要另外装驱动
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.vgListern = new Vguang();
            this.vgListern.OpenVgDevice();

            //设置扫码成功时的回调
            this.vgDecodeCall = new Vguang.DecodeCallBack(vgDecodeCallBackStr);
            Vguang.setDecodeCallBack(vgDecodeCall);

            if (ConfigHelper.GetConfigInfo() == false)
            {
                System.Windows.MessageBox.Show("错误!请检查配置文件中 Https.url/Authority 是否有进行设置?\n设置完毕后，请重新启动该应用程序！");
            }

            //不显示界面，最小化运行
            this.Hide();
            this.ShowInTaskbar = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.vgListern.CloseVgDevice();
        }

        public void ReceiveVgBarcode(string barcode) 
        {
            // 由于非UI线程不能亲自刷新UI的内容，所以需要通知UI线程进行更新
            // 不然这里会产生调用线程无法访问此对象，因为另一个线程拥有该对象
            Dispatcher.Invoke(
                new Action(
                    delegate
                    {
                        // 条形码显示
                        this.textBox_DisplayKeyboardInput.Text = barcode;
                        // 本地调试需要注释掉
                        StartMonitor(barcode);
                    }
                )
             );
        }

        //扫码成功时的回调函数
        private int vgDecodeCallBackStr(IntPtr str, int length)
        {
            //得到解码结果字符串
            string result = Marshal.PtrToStringAnsi(str);
            ReceiveVgBarcode(result);

            return 0;
        }
#endif

        void StartMonitor(string barcode)
        {
            if (Utils.MmmerServerIsRunning())
            {
                //如果配置文件中有接种证的测试编号，则使用测试文件中的数据
                if (ConfigHelper.GetConfigTestImuno() != null)
                {
                    string[] testImuno = ConfigHelper.GetConfigTestImuno().Split(',');
                    foreach (string item in testImuno)
                    {
                        GetConfirmResult(item);
                    }
                }
                else
                {
                    GetConfirmResult(barcode);
                }
            }
        }

        /// <summary>
        /// 接种证编号，需要从扫码枪中获得
        /// </summary>
        /// <param name="vaccinationNum"></param>
        private void GetConfirmResult(string vaccinationNum)
        {
            string retStr = Utils.GetJsonResult(vaccinationNum);
            if (retStr != null)
            {
                this.babyCreateResult.baseInfo = null;
                this.babyCreateResult.babyInfo = null;
                Utils.cBabyInfo baby = new Utils.cBabyInfo();

                //数据进行解析
                Utils.DataParse(retStr, ref baby);
                this.babyCreateResult.baseInfo = Utils.CreateInfo(baby);
                this.babyCreateResult.babyInfo = baby.GetBabyInfo();
                
                if (this.babyCreateResult.baseInfo != null)
                {
                    UsbPrint();
                }

                //显示在界面上 
                this.textBlock_name.Text = baby.m_BabyName;
                this.textBlock_time.Text = baby.m_BabyTime;
                this.textBlock_code.Text = Convert.ToString((int)baby.m_iErrorCode);
            }
        }

        private void UsbPrint()
        {
            try
            { 
                // 使用PrintDocument来进行打印
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(this.PrintInfo);
                pd.Print();
            }
            catch (Exception ex)
            {
                // 捕获异常，出包的时候这里要去掉
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void PrintInfo(object sender, PrintPageEventArgs ev)
        {
            // 字体颜色
            var printColor = System.Drawing.Brushes.Black;
            // 票面通用字体
            var printNormalFont = new Font("宋体", 12, System.Drawing.FontStyle.Regular);
            // 票面数字号码要大一些
            var serverNumberFont = new Font("宋体", 22, System.Drawing.FontStyle.Bold);
            // 公司名称字体 
            var companyFont = new Font("宋体", 12, System.Drawing.FontStyle.Regular);
            // 卫生院名称字体
            var hospitalFont = new Font("宋体", 20, System.Drawing.FontStyle.Regular);

#if true
            string hospital = ConfigHelper.GetConfigAuthorityName();
            const string company = "微象科技\n";
            const string server = "办理业务:";
            const string window = "办理窗口:";
            const string waitingPerson = "等待人数:";
            const string time = "取号时间:";
            const string name = "姓名:";
            // 拼接字符串
            string serverNumber = Convert.ToString(this.babyCreateResult.baseInfo.Code);
            string printServer = server + Convert.ToString(this.babyCreateResult.baseInfo.ServiceName);
            string printWindow = window + Convert.ToString(this.babyCreateResult.baseInfo.Windows);
            string printWaitingPerson = waitingPerson + Convert.ToString(this.babyCreateResult.baseInfo.Waiting);
            string printTime = time + string.Format("{0}/{1}/{2} {3}:{4}:{5}", this.babyCreateResult.baseInfo.Fcd.Year, this.babyCreateResult.baseInfo.Fcd.Month,
                                                    this.babyCreateResult.baseInfo.Fcd.Day, this.babyCreateResult.baseInfo.Fcd.Hour,
                                                    this.babyCreateResult.baseInfo.Fcd.Minute, this.babyCreateResult.baseInfo.Fcd.Second);
            string printName = name + this.babyCreateResult.babyInfo.m_BabyName;
#else
            string hospital = ConfigHelper.GetConfigAuthorityName();
            const string company = "牛逼科技\n";
            string serverNumber = "A987";
            string printServer = "办理业务:A";
            string printWindow = "办理窗口:1";
            string printWaitingPerson = "等待人数:15";
            string printTime = "取号时间:2016/9/11 12:21:33";
            string printName = "姓名:王二";
#endif

            // 公司名字起始位置
            var pointCompanyX = 90f;
            // 取到的号码的起始位置
            var pointNumberX = 90f;
            // 其他信息字符串的起始位置
            var pointNormalX = 10f;
            // 起始时的Y轴位置
            var pointY = 0f;

            // 如果存在图片，则进行打印
            if (ConfigHelper.GetConfigLogoPath() != null)
            {
                System.Drawing.Image newImage;
                Boolean needDrawPic = true;
                try
                {
                    System.Drawing.Image.FromFile(ConfigHelper.GetConfigLogoPath());
                }
                catch (Exception ex)
                {
                    // OutOfMemoryException，FileNotFoundException，ArgumentException
                    // 截取异常，图片文件可能不存在，那就不需要进行打印 
                    needDrawPic = false;
                }

                if (needDrawPic)
                {
                    newImage = System.Drawing.Image.FromFile(ConfigHelper.GetConfigLogoPath());
                    ev.Graphics.DrawImage(newImage, 0, 0);
                }
            }

            // 存在医院信息则进行打印
            if (hospital != null)
            {
                SizeF hospitalSize = TextRenderer.MeasureText(hospital, hospitalFont);
                StringFormat hopitalStrFormat = new StringFormat();
                // 水平居中
                hopitalStrFormat.Alignment = StringAlignment.Center;
                ev.Graphics.DrawString(hospital, hospitalFont, printColor, new RectangleF(0, 0, 290, hospitalSize.Height), hopitalStrFormat);
            }

            // 取到的号
            ev.Graphics.DrawString(serverNumber, serverNumberFont, printColor, pointNumberX, pointY += 30f);
            // 业务
            ev.Graphics.DrawString(printServer, printNormalFont, printColor, pointNormalX, pointY += 30f);
            // 窗口
            ev.Graphics.DrawString(printWindow, printNormalFont, printColor, pointNormalX, pointY += 20f);
            // 等待人数
            ev.Graphics.DrawString(printWaitingPerson, printNormalFont, printColor, pointNormalX, pointY += 20f);
            // 取号时间
            ev.Graphics.DrawString(printTime, printNormalFont, printColor, pointNormalX, pointY += 20f);
            // 姓名
            ev.Graphics.DrawString(printName, printNormalFont, printColor, pointNormalX, pointY += 20f);
            // 公司名字
            ev.Graphics.DrawString(company, companyFont, printColor, pointCompanyX, pointY += 20f); 
        }

        /// <summary>
        /// 按钮控制进行打印测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(this.PrintInfo);
                pd.Print();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 测试打印回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        //private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        //{
        //    // 字体颜色
        //    var printColor = System.Drawing.Brushes.Black;
        //    // 票面通用字体
        //    var printNormalFont = new Font("宋体", 12, System.Drawing.FontStyle.Regular);
        //    // 票面数字号码要大一些
        //    var serverNumberFont = new Font("宋体", 25, System.Drawing.FontStyle.Bold);
        //    // 公司名称字体 
        //    var companyFont = new Font("宋体", 15, System.Drawing.FontStyle.Regular);
        //    // 卫生院名称字体
        //    var hospitalFont = new Font("宋体", 20, System.Drawing.FontStyle.Regular);

        //    string hospital = ConfigHelper.GetConfigAuthorityName();
        //    const string company = "牛逼科技\n";
        //    string serverNumber = "A987";
        //    string printServer = "办理业务:A";
        //    string printWindow = "办理窗口:1";
        //    string printWaitingPerson = "等待人数:15";
        //    string printTime = "取号时间:2016/9/11 12:21:33";
        //    string printName = "姓名:王二";

        //    var pointCompanyX = 90f;
        //    var pointNumberX = 90f;
        //    var pointNormalX = 10f;
        //    var pointY = 0f;

        //    // 如果存在图片，则进行打印
        //    if (ConfigHelper.GetConfigLogoPath() != null)
        //    {
        //        System.Drawing.Image newImage = System.Drawing.Image.FromFile(ConfigHelper.GetConfigLogoPath());
        //        ev.Graphics.DrawImage(newImage, 0, 0);
        //    }

        //    // 存在医院信息则进行打印
        //    if (hospital != null)
        //    {
        //        SizeF hospitalSize = TextRenderer.MeasureText(hospital, hospitalFont);
        //        StringFormat hopitalStrFormat = new StringFormat();
        //        // 水平居中
        //        hopitalStrFormat.Alignment = StringAlignment.Center;
        //        ev.Graphics.DrawString(hospital, hospitalFont, printColor, new RectangleF(0, 0, 290, hospitalSize.Height), hopitalStrFormat);
        //    }
            
        //    // 取到的号
        //    ev.Graphics.DrawString(serverNumber, serverNumberFont, printColor, pointNumberX, pointY += 30f);
        //    // 业务
        //    ev.Graphics.DrawString(printServer, printNormalFont, printColor, pointNormalX, pointY += 30f);
        //    // 窗口
        //    ev.Graphics.DrawString(printWindow, printNormalFont, printColor, pointNormalX, pointY += 20f);
        //    // 等待人数
        //    ev.Graphics.DrawString(printWaitingPerson, printNormalFont, printColor, pointNormalX, pointY += 20f);
        //    // 取号时间
        //    ev.Graphics.DrawString(printTime, printNormalFont, printColor, pointNormalX, pointY += 20f);
        //    // 姓名
        //    ev.Graphics.DrawString(printName, printNormalFont, printColor, pointNormalX, pointY += 20f);
        //    // 公司名字
        //    ev.Graphics.DrawString(company, companyFont, printColor, pointCompanyX, pointY += 20f);  
        //}
    }
}
