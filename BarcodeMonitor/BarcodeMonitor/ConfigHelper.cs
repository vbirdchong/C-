using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BarcodeMonitor
{
    class ConfigHelper
    {
        private static string m_configUrl;
        private static string m_authorityCode;
        private static string m_testImuno;
        private static string m_serviceCode;
        private static string m_authorityName;
        private static string m_logoPath;

        /// <summary>
        /// 返回*.exe.config 文件中appSetting配置段中对应的value值
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        private static string GetAppConfig(string strKey)
        {
            foreach (string key in System.Configuration.ConfigurationManager.AppSettings)
            {
                if (key == strKey)
                {
                    return ConfigurationManager.AppSettings[strKey];
                }
            }
            return null;
        }

        public static bool GetConfigInfo()
        {
            //根据传入的接种证编号和配置文件中机构代码 创建URL地址
            m_configUrl = GetAppConfig("Https.url");
            m_authorityCode = GetAppConfig("Authority");
            m_testImuno = GetAppConfig("TestImuno");
            m_serviceCode = GetAppConfig("ServiceCode");
            m_authorityName = GetAppConfig("AuthorityName");
            m_logoPath = GetAppConfig("LogoPath");

            if (m_configUrl == null || m_authorityCode == null)
            {
                //Console.WriteLine("错误!请检查配置文件中 Https.url/Authority 是否有进行设置");
                return false;
            }
            return true;
        }

        public static string GetConfigUrl()
        {
            //Console.WriteLine("GetConfigUrl:{0}", m_configUrl);
            return m_configUrl;
        }

        public static string GetConfigAuthorityCode()
        {
            //Console.WriteLine("m_authorityCode:{0}", m_authorityCode);
            return m_authorityCode;
        }

        public static string GetConfigTestImuno()
        {
            //Console.WriteLine("m_testImuno:{0}", m_testImuno);
            return m_testImuno;
        }

        public static string GetConfigServiceCode()
        {
            //Console.WriteLine("m_serviceCode:{0}", m_serviceCode);
            return m_serviceCode;
        }

        public static string GetConfigAuthorityName()
        {
            return m_authorityName;
        }

        public static string GetConfigLogoPath()
        {
            return m_logoPath;
        }
    }
}
