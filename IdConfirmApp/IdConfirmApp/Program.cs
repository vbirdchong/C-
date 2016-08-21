using System;
using System.IO;
using System.Runtime.InteropServices;   //调用WINDOWS API函数时要用到
using Microsoft.Win32;  //写入注册表时要用到
using System.Windows.Forms;

namespace IdConfirmApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //延时启动，Mmmer进程可能没有起来
            //Utils.DelayTime(60);
            ConfigHelper.GetConfigInfo();
            while (true)
            {
                Utils.DelayTime(2);
                StartMonitor();
            }
        }

        static void StartMonitor()
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
                    // to do 使用扫描枪的结果
                    string barCode = Console.ReadLine();
                    Console.WriteLine(barCode);
                    GetConfirmResult(barCode);
                }
            }
        }

        /// <summary>
        /// 接种证编号，需要从扫码枪中获得
        /// </summary>
        /// <param name="vaccinationNum"></param>
        static void GetConfirmResult(string vaccinationNum)
        {
            string retStr = Utils.GetJsonResult(vaccinationNum);
            if (retStr != null)
            {
                Utils.cBabyInfo baby = new Utils.cBabyInfo();
                //数据进行解析
                Utils.DataParse(retStr, ref baby);
                baby.PrintBabyInfo();
                Utils.CreateInfo(baby);
            }
        }


    }
}
