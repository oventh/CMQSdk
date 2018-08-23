using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Cmq_SDK.Exception;

namespace Cmq_SDK.Cmq
{
    class CmqClient
    {
        private  string CURRENT_VERSION = "SDK_C#_1.0";
        
        private string secretId;
        private string secretKey;
        private string endpoint;
        private string path;
        private HttpClient httpClient;
        private string method;
        private int timeout;    //http timeout milseconds
        private string signMethod;

        public void setHttpMethod(string value)
        {
            if (value.ToUpper() == "POST" || value.ToUpper() == "GET")
            {
                    this.method = value.ToUpper();
             }
            else
            {
                    throw new ClientException("http method only support POST and GET");
             }
         }

        public void setSignMethod(string value)
        {
            if (value.ToUpper() == "HMACSHA1" || value.ToUpper() == "HMACSHA256")
                this.signMethod = value.ToUpper();
            else
                throw new ClientException("signMethod only can be HmacSHA1 or HmacSHA256");
        }

         
        public void setTimeout(int value)
        {
            this.timeout = value;
        }
        

        public CmqClient(string secretId, string secretKey, string endpoint, string path, string method)
        {
            this.secretId = secretId;
            this.secretKey = secretKey;
            this.endpoint = endpoint;
            if (!(endpoint.StartsWith("http://")  || endpoint.StartsWith("https://")))
                throw new ClientException("endpoint only support http or https");
            this.path = path;
            this.method = method;
            this.signMethod = "HMACSHA1";
            this.timeout = 10000;       //10s
            this.httpClient = new HttpClient();
            return;
        }

        public string call(string action, SortedDictionary<string, string> param)
        {
            string rsp;
            try
            {
                Random ran = new Random();
                int nonce = ran.Next(int.MaxValue);
                param.Add("Action", action);
                param.Add("Nonce", Convert.ToString(new Random().Next(int.MaxValue)));
                param.Add("SecretId", this.secretId);
                int timestamp = (int) (((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalSeconds);
                param.Add("Timestamp", Convert.ToString(timestamp));
                param.Add("RequestClient", this.CURRENT_VERSION);
                if (this.signMethod.ToUpper() == "HMACSHA1")
                    param.Add("SignatureMethod", "HmacSHA1");
                else
                    param.Add("SignatureMethod", "HmacSHA256");

                string host = "";
                if (this.endpoint.StartsWith("https"))
                    host = this.endpoint.Substring(8);
                else
                    host = this.endpoint.Substring(7);
                string src = "";
                src += this.method + host + this.path + "?";

                bool flag = false;
                string[] keysArray = new string[param.Keys.Count];
                param.Keys.CopyTo(keysArray, 0);
                Array.Sort(keysArray, string.CompareOrdinal);
                foreach (string key in keysArray)
                {
                    if (flag)
                        src += "&";
                    src += key.Replace("_", ".") + "=" + param[key];
                    flag = true;
                }

                param.Add("Signature", Sign.Signature(src, this.secretKey, this.signMethod));

                string url = "";
                string req = "";
                if (this.method.ToUpper() == "GET")
                {
                    url = this.endpoint + this.path + "?";
                    flag = false;
                    foreach (string key in param.Keys)
                    {
                        if (flag)
                            url += "&";
                        url += key + "=" + HttpUtility.UrlEncode(param[key]);
                        flag = true;
                    }

                    if (url.Length > 2048)
                        throw new ClientException("URL length is larger than 2K when use the GET method ");
                }
                else
                {
                    url = this.endpoint + this.path;
                    flag = false;
                    foreach (String key in param.Keys)
                    {
                        if (flag)
                            req += "&";
                        req += key + "=" + HttpUtility.UrlEncode(param[key]);
                        flag = true;
                    }
                }
                rsp = this.httpClient.sendRequest(this.method, url, req, this.timeout);
            }
            catch (System.Exception e) {
                throw e;
            }
            return rsp;



        }

        

        

        


    }
};
