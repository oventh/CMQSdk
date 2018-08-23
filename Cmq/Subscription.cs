using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cmq_SDK.Exception;


namespace Cmq_SDK.Cmq
{
    public class Subscription
    {
        private string topicName;
        private string subscriptionName;
        private CmqClient client;
        internal Subscription(string topicName, string subscriptionName, CmqClient client)
        {
            this.topicName = topicName;
            this.subscriptionName = subscriptionName;
            this.client = client;
        }
        public void  clearFilterTags()
        {
 
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            param.Add("subscriptionName", this.subscriptionName);

            string result = this.client.call("ClearSUbscriptionFIlterTags", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }    

        public void  setSubscriptionAttributes(SubscriptionMeta meta)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            param.Add("subscriptionName", this.subscriptionName);
            if( meta.notifyStrategy != "")
                param.Add("notifyStrategy",meta.notifyStrategy);
            if( meta.notifyContentFormat != "")
                param.Add("notifyContentFormat",meta.notifyContentFormat);
            if(meta.filterTag != null)
            {
                for(int i =0; i< meta.filterTag.Count ; ++i)
                    param.Add("filterTag." + Convert.ToString(i),meta.filterTag[i]);
            }
            if(meta.bindingKey!=null)
            {
                for(int i = 0 ; i< meta.bindingKey.Count ; ++i)
                    param.Add("bindingKey."+Convert.ToString(i),meta.bindingKey[i]);
            }

            string result = this.client.call("SetSubscriptionAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public SubscriptionMeta getSubscriptionAttributes()
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            param.Add("subscriptionName", this.subscriptionName);

            string result = this.client.call("getSubscriptionAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());

            SubscriptionMeta meta = new SubscriptionMeta();


            meta.filterTag = new  List<string>();
            meta.bindingKey =new  List<string>();

            meta.endpoint = (string)jObj["endpoint"];
            meta.notifyStrategy = (string)jObj["notifyStrategy"];
            meta.notifyContentFormat = (string)jObj["notifyContentFormat"];
            meta.protocal = (string)jObj["protocol"];
            meta.createTime = (int)jObj["createTime"];
            meta.lastModifyTime = (int)jObj["lastModifyTime"];
            meta.msgCount = (int)jObj["msgCount"];

            JArray filterTagArray = JArray.Parse(jObj["filterTag"].ToString());
            foreach (var item in filterTagArray) {
                meta.filterTag.Add(item.ToString());
            }

            JArray bindingKeyArray = JArray.Parse(jObj["bindingKey"].ToString());
            foreach(var item in bindingKeyArray){
                meta.bindingKey.Add(item.ToString());
            }

            return  meta;
           
        }
    }
}
