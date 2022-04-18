using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace InstallPackage
{
    class Downloader
    {
        public string _prog;
        public string _link;
        public string _path;
        public bool   _bCompleted = false;
        public bool   _bError = false;
        public int   _total_bytes;
        public int   _received_bytes;
        private ProgressBar _prgsbar;
        private bool _b_executable;

        public bool  start(string prog, string link, ProgressBar prgsbar, bool bExe)
        {
            this._prgsbar = prgsbar;
            this._link = link;
            this._prog = prog;
            this._b_executable = bExe;
            string tempDir = Path.GetTempPath();
            string tempFile = Path.GetFileNameWithoutExtension(link) + "_" + DateTime.Now.Ticks.ToString();
            this._path = Path.Combine(tempDir, tempFile);

            if (bExe)
                this._path = this._path + ".exe";
            else
                this._path = this._path + ".msi";
            this.startDownload();
            return true;
        }
        private void startDownload()
        {
            if(this._prgsbar!=null)
            {
                this._prgsbar.Minimum = 0;
                this._prgsbar.Maximum = 100;
                this._prgsbar.Value = 0;
            }
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri(this._link), this._path);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this._total_bytes = int.Parse(e.TotalBytesToReceive.ToString());
            this._received_bytes = int.Parse(e.BytesReceived.ToString());
            int percentage = (int)((double)this._received_bytes / (double)(this._total_bytes) * 100.0);
            percentage = Math.Min(percentage, 100);
            if (this._prgsbar != null)
            {
                this._prgsbar.Value = percentage;
            }
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this._bCompleted = true;
            if(_b_executable)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = this._path;
                proc.StartInfo.Verb = "runas";
                try
                {
                    proc.Start();
                    proc.WaitForExit();
                }
                catch(Exception ee)
                {

                }                
            }
            else
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "msiexec";
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(this._path);
                proc.StartInfo.Arguments = " /i \"" + this._path + "\"";
                proc.StartInfo.Verb = "runas";                


                try
                {
                    proc.Start();
                    proc.WaitForExit();
                }
                catch (Exception ee)
                {

                }
            }
        }
    }
}
