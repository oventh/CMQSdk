using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cmq_SDK.Exception;
namespace Cmq_SDK.Cmq
{
    public class Topic
    {
        private string topicName;
        private CmqClient client;
        internal Topic(string topicName, CmqClient client) {
            this.topicName = topicName;
            this.client = client;
        }

        public void setTopicAttributes(int maxMsgSize) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            if(maxMsgSize < 0 || maxMsgSize > 65536)
                throw new ClientException("Invalid parameter maxMsgSize < 0 or maxMsgSize > 65536");
            param.Add("maxMsgSize", Convert.ToString(maxMsgSize));

            string result = this.client.call("SetTopicAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }


        public TopicMeta getTopicAttributes() {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);

            string result = this.client.call("GetTopicAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            TopicMeta meta = new TopicMeta();
            meta.msgCount = (int)jObj["msgCount"];
            meta.maxMsgSize = (int)jObj["maxMsgSize"];
            meta.msgRetentionSeconds =(int)jObj["msgRetentionSeconds"];
            meta.createTime = (int)jObj["createTime"];
            meta.lastModifyTime =(int)jObj["lastModifyTime"];
            meta.filterType = (int)jObj["filterType"];
            return meta;
        }

        public string publishMessage(string msgBody) {
            return publishMessage(msgBody, new List<string>()," ");
        }
        public string publishMessage(string msgBody,string routingKey){
            return publishMessage(msgBody,new List<string>(),routingKey);
        }
        public string publishMessage(string msgBody, List<string> tagList,string routingKey)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            param.Add("msgBody", msgBody);
            if (routingKey != "")
                param.Add("routingKey",routingKey);

            if(tagList != null)
            {
                for (int i = 0; i < tagList.Count; i++) {
                    string k = "msgTag." + Convert.ToString(i + 1);
                    param.Add(k, tagList[i]);
                }
            }
            string result = this.client.call("PublishMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return jObj["msgId"].ToString();

        }

        public List<string> batchPublishMessage(List<string>vtMsgBody)
        {
            return batchPublishMessage(vtMsgBody,new List<string>(),"");
        }
        public List<string> batchPublishMessage(List<string> vtMsgBody,string routingKey)
        {
            return batchPublishMessage(vtMsgBody,new List<string>(),routingKey);
        }
        public List<string> batchPublishMessage(List<string> vMsgBody,List<string> vTagList,string routingKey) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);

            if(routingKey!=null)
                param.Add("routingKey",routingKey);
            if(vMsgBody != null)
            {
                for (int i = 0; i < vMsgBody.Count; i++) {
                    string k = "msgBody." + Convert.ToString(i + 1);
                    param.Add(k, vMsgBody[i]);
                }
            }
            if(vTagList != null)
            {
                for(int i = 0 ; i < vTagList.Count; i++){
                    string k = "msgTag"+Convert.ToString(i+1);
                    param.Add(k,vTagList[i]);
                }                
            }

            string result = this.client.call("BatchPublishMessage", param);

            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            List<string> vMsgId = new List<string>();
            JArray idsArray = JArray.Parse(jObj["msgList"].ToString());
            foreach (var item in idsArray)
            {
                vMsgId.Add(item["msgId"].ToString());
            }
            return vMsgId;

        }

        public int ListSubscription(int offset, int limit , string searchWord, List<string> subscriptionList)
        {
             SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("topicName", this.topicName);
            if( searchWord != "")
                param.Add("searchWord",searchWord);
            if(offset >= 0)
                param.Add("offset",Convert.ToString(offset));
            if(limit > 0)
                param.Add("limit",Convert.ToString(limit));

            string result = this.client.call("ListSubscriptionByTopic", param);

            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            int totalCount = (int)jObj["totalCount"];
            List<string> vMsgId = new List<string>();
            JArray idsArray = JArray.Parse(jObj["subscriptionList"].ToString());
            foreach (var item in idsArray)
            {
                subscriptionList.Add(item["subscriptionName"].ToString());
            }
            return totalCount;
        }

       
    }
}
