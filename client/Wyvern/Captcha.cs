using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Wyvern
{
    class Captcha
    {
        public string hCaptcha(string key, string sitekey, string pageurl)
        {
            try
            {
                String callUrl = "https://2captcha.com/in.php";
                String postData = String.Format("key={0}&method=hcaptcha&sitekey={1}&pageurl={2}", key, sitekey, pageurl);

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
                byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = sendData.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(sendData, 0, sendData.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                if (res.Contains("OK|"))
                {
                    return res.Split('|')[1];
                }
                else
                {
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "ERROR";
            }
        }

        public string reCaptcha(string key, string sitekey, string pageurl)
        {
            try
            {
                String callUrl = "https://2captcha.com/in.php";
                String postData = String.Format("key={0}&method=userrecaptcha&googlekey={1}&pageurl={2}", key, sitekey, pageurl);

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
                byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = sendData.Length;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(sendData, 0, sendData.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string res = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebResponse.Close();
                if (res.Contains("OK|"))
                {
                    return res.Split('|')[1];
                }
                else
                {
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "ERROR";
            }
        }

        public string resCaptcha(string key, string id)
        {
            StringBuilder getParams = new StringBuilder();
            getParams.Append("key=" + key);
            getParams.Append("&action=" + "get");
            getParams.Append("&id=" + id);
        retry:
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("https://2captcha.com/res.php?" + getParams);
            myReq.Method = "GET";
            HttpWebResponse wRes = (HttpWebResponse)myReq.GetResponse();
            Stream respGetStream = wRes.GetResponseStream();
            StreamReader readerGet = new StreamReader(respGetStream, Encoding.UTF8);
            string res = readerGet.ReadToEnd();
            if (res.Equals("CAPCHA_NOT_READY"))
            {
                Thread.Sleep(10000);
                goto retry;
            }
            if (res.Contains("OK|"))
            {
                return res.Split('|')[1];
            }
            else
            {
                return res;
            }
        }

        
    }
}
