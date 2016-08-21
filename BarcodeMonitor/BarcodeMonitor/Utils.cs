using Mmmer.Queuer.Integration.Sao;
using Mmmer.Queuer.Transfering.Dto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BarcodeMonitor
{
    class Utils
    {
        public enum ErrorCodeType
        {
            BABY_OK = 200,          // 已经预约
            BABY_NO_Appoint = 404,  // 未预约，但在服务中存在该baby信息
            BABY_NO_RECORD = 500,   // 未预约，也无该baby信息
        }

        public class cBabyInfo
        {
            // 服务器的返回信息中携带了:姓名、时间段、返回码
            public string m_BabyName;
            public string m_BabyTime;
            public ErrorCodeType m_iErrorCode;

            public cBabyInfo()
            {
                m_BabyName = "";
                m_BabyTime = "";
                m_iErrorCode = ErrorCodeType.BABY_NO_RECORD;
            }

            public cBabyInfo(string babyName, string babyTime, ErrorCodeType errorCode)
            {
                this.m_BabyName = babyName;
                this.m_BabyTime = babyTime;
                this.m_iErrorCode = errorCode;
            }

            public cBabyInfo GetBabyInfo()
            {
                return this;
            }

            public void PrintBabyInfo()
            {
                Console.WriteLine("BabyName:{0}\nCode:{1}\nTime:{2}\n\n\n", m_BabyName, (int)m_iErrorCode, m_BabyTime);
            }
        }

        /// <summary>
        /// 向叫号系统添加人员信息，即进行取号
        /// </summary>
        /// <param name="baby"></param>
        public static void CreateInfo(cBabyInfo baby)
        {
            string serviceCode = "A";
            if (ConfigHelper.GetConfigServiceCode() != null)
            {
                serviceCode = ConfigHelper.GetConfigServiceCode();
            }

            if (MmmerServerIsRunning())
            {
                ServiceDto dto1 = SaoFactory.QueuerClientSinglethon.GetServiceByCode(serviceCode);
                QueueDto dtoX = new QueueDto();
                dtoX.Service = dto1.ID;

                if (baby.m_iErrorCode == ErrorCodeType.BABY_NO_RECORD)
                {
                    //dtoX.Phone = "无预约";
                    dtoX.IDCard = "";
                    dtoX.Name = "";
                }
                else
                {
                    dtoX.Name = baby.m_BabyName;
                    dtoX.IDCard = "";
                    // 该宝宝已经有预约
                    // 对于phone这个字段是否可以拿来复用还未定，暂时将其注释掉

                    //if (baby.m_iErrorCode == ErrorCodeType.BABY_OK)
                    //{
                    //    dtoX.Phone = baby.m_BabyTime;
                    //}
                    //else
                    //{
                    //    dtoX.Phone = "无预约";
                    //}
                }

                TicketDto dto2 = SaoFactory.QueuerClientSinglethon.Create(dtoX);
                //Console.WriteLine(string.Format("Code:{0},ServiceName:{1},Windows:{2},Waiting:{3}", dto2.Code, dto2.ServiceName, dto2.Windows, dto2.Waiting));
            }
        }


        /// <summary>
        /// 将json中的数据进行解析，并通过参数返回
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="baby"></param>
        public static void DataParse(string jsonStr, ref cBabyInfo baby)
        {
            JObject o = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);
            IEnumerable<JProperty> propertys = o.Properties();
            foreach (JProperty item in propertys)
            {
                if (item.Name == "BabyName")
                {
                    baby.m_BabyName = (string)item.Value;
                }

                if (item.Name == "Code")
                {
                    baby.m_iErrorCode = (ErrorCodeType)Convert.ToInt32(item.Value);
                }

                if (item.Name == "Time")
                {
                    baby.m_BabyTime = (string)item.Value;
                }
            }
        }

        /// <summary>
        /// 简单的秒级延时函数
        /// </summary>
        /// <param name="seconds"></param>
        public static void DelayTime(double seconds)
        {
            DateTime tempTime = DateTime.Now;
            while (tempTime.AddSeconds(seconds).CompareTo(DateTime.Now) > 0)
            {
                // 在WPF中无效
                //Application.DoEvents();
            }
        }

        /// <summary>
        /// 检查叫号服务程序是否启动
        /// </summary>
        /// <returns></returns>
        public static bool MmmerServerIsRunning()
        {
            string processName = "Mmmer.Queuer.Svc";
            if (GetPidByProcessName(processName) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 从服务器端获取json数据，并转化成字符串进行返回
        /// </summary>
        /// <param name="vaccinationNum"></param>
        /// <returns></returns>
        public static string GetJsonResult(string vaccinationNum)
        {
            if (ConfigHelper.GetConfigUrl() == null || ConfigHelper.GetConfigAuthorityCode() == null)
            {
                return null;
            }

            string requestUrl = ConfigHelper.GetConfigUrl() + "?code="
                        + ConfigHelper.GetConfigAuthorityCode() + "&imuno="
                        + vaccinationNum;
            //Console.WriteLine("Url: {0}", requestUrl);

            //发送请求并获取结果
            HttpWebResponse webRsp = HttpHelper.CreateGetHttpResponse(requestUrl, 0, null, null);
            string retStr = HttpHelper.GetResponseString(webRsp);
            //Console.WriteLine("result:{0}", retStr);
            return retStr;
        }

        /// <summary>
        /// 查看进程是否启动，未启动则返回0
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            foreach (Process p in arrayProcess)
            {
                return p.Id;
            }
            return 0;
        }
    }
}
