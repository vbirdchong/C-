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
using DesktopWPFAppLowLevelKeyboardHook;

namespace BarcodeMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private LowLevelKeyboardListener _listener;
        private NotifyIcon notifyIcon;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.BarCodeEvent += _listener_OnKeyPressed;
            _listener.HookKeyboard();
            if (ConfigHelper.GetConfigInfo() == false)
            {
                System.Windows.MessageBox.Show("错误!请检查配置文件中 Https.url/Authority 是否有进行设置?\n设置完毕后，请重新启动该应用程序！");
            }
        }

        void _listener_OnKeyPressed(LowLevelKeyboardListener.BarcodeInfo barCodeInfo)
        {

            //System.Windows.MessageBox.Show(Convert.ToString(barCodeInfo.strBarcode.Length));
            
            // 条形码显示
            this.textBox_DisplayKeyboardInput.Text = barCodeInfo.strBarcode;
            StartMonitor(barCodeInfo.strBarcode);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

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
        void GetConfirmResult(string vaccinationNum)
        {
            string retStr = Utils.GetJsonResult(vaccinationNum);
            if (retStr != null)
            {
                Utils.cBabyInfo baby = new Utils.cBabyInfo();
                //数据进行解析
                Utils.DataParse(retStr, ref baby);
                Utils.CreateInfo(baby);
                this.textBlock_name.Text = baby.m_BabyName;
                this.textBlock_time.Text = baby.m_BabyTime;
                this.textBlock_code.Text = Convert.ToString((int)baby.m_iErrorCode);
            }
        }
    }
}
