using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
namespace Cmq_SDK.Cmq
{
    internal class HttpClient
    {
        private bool isKeepAlive;
        private string url;
        private HttpWebRequest connect;
        
        public HttpClient() {
            this.isKeepAlive = true;
            this.url = "";
            this.connect = null;
        }
        private void newHttpConnection(string url, int timeout) {
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            }
            this.url = url;
            this.connect = (HttpWebRequest)HttpWebRequest.Create(this.url);
            
            this.connect.Accept = "*/*";
            this.connect.KeepAlive = this.isKeepAlive;
            this.connect.Timeout = timeout;
            this.connect.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)";
            ServicePointManager.Expect100Continue = false;
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        public string sendRequest(string method, string url, string request, int timeout) {

            newHttpConnection(url, timeout);
            this.connect.Timeout = timeout;
            if (method.ToUpper() == "POST") {
                this.connect.Method = "POST";
                this.connect.ContentType = "application/x-www-form-urlencoded";
                var paramsByte = Encoding.GetEncoding("utf-8").GetBytes(request);
                this.connect.ContentLength = paramsByte.Length;
                Stream requestStream = this.connect.GetRequestStream();
                {
                    requestStream.Write(paramsByte, 0, paramsByte.Length);
                    requestStream.Close();
                }
                

            }
            string result;
            using (HttpWebResponse response = (HttpWebResponse)this.connect.GetResponse())
            {
                using (var s = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(s, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                        reader.Close();
                        s.Close();
                        response.Close();
                    }
                }
            }
            return result;






        }

    }
}
