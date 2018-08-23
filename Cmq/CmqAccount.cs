using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Cmq_SDK.Exception;

namespace Cmq_SDK.Cmq
{
    public class CmqAccount
    {
        private CmqClient client;

        public CmqAccount(string endpoint, string secretId, string secretKey) {
            this.client = new CmqClient(secretId, secretKey, endpoint, "/v2/index.php", "POST");
        }

        public void setSignMethod(string signMethod) {
            this.client.setSignMethod(signMethod);
        }
        public void setHttpMethod(string method) {
            this.client.setHttpMethod( method);
        }
        public void setTimeout(int timeout) {
            //timeout is milseconds for the http request
            this.client.setTimeout(timeout);
        }

        public int listQueue(string searchWord, int offset, int limit,  List<string> queueList) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (!searchWord.Equals(""))
                param.Add("searchWord", searchWord);
            if (offset >= 0)
                param.Add("offset", Convert.ToString(offset));
            if (limit > 0)
                param.Add("limit", Convert.ToString(limit));

            string result = this.client.call("ListQueue", param);

            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            int totalCount = (int)jObj["totalCount"];
            JArray queueListArray = JArray.Parse(jObj["queueList"].ToString());
            foreach (var item in queueListArray) {
                queueList.Add(item["queueName"].ToString());
            }
            return totalCount;
  
        }



        public void createQueue(string queueName, QueueMeta meta) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (queueName == "")
                throw new ClientException("Invalid parameter: queueName is empty");
            else
                param.Add("queueName", queueName);

            if (meta.maxMsgHeapNum > 0)
                param.Add("maxMsgHeapNum", Convert.ToString(meta.maxMsgHeapNum));
            if (meta.pollingWaitSeconds > 0)
                param.Add("pollingWaitSeconds", Convert.ToString(meta.pollingWaitSeconds));
            if (meta.visibilityTimeout > 0)
                param.Add("visibilityTimeout", Convert.ToString(meta.visibilityTimeout));
            if (meta.maxMsgSize > 0)
                param.Add("maxMsgSize", Convert.ToString(meta.maxMsgSize));
            if (meta.msgRetentionSeconds > 0)
                param.Add("msgRetentionSeconds", Convert.ToString(meta.msgRetentionSeconds));

            string result = this.client.call("CreateQueue", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public void deleteQueue(string queueName) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (queueName == "")
                throw new ClientException("Invalid parameter: queueName is empyt");
            else
                param.Add("queueName", queueName);
            string result = this.client.call("DeleteQueue", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public Queue getQueue(string queueName) {
            return new Queue(queueName, this.client);
        }


        public int listTopic(string searchWord, int offset, int limit,  List<string> topicList) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (!searchWord.Equals(""))
                param.Add("searchWord", searchWord);
            if (offset >= 0)
                param.Add("offset", Convert.ToString(offset));
            if (limit > 0)
                param.Add("limit", Convert.ToString(limit));

            string result = this.client.call("ListTopic", param);

            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            int totalCount = (int)jObj["totalCount"];
            JArray topicListArray = JArray.Parse(jObj["topicList"].ToString());
            foreach (var item in topicListArray) {
                topicList.Add(item["topicName"].ToString());
            }
            return totalCount;
  
        }
        public Topic getTopic (string topicName)
        {
            return new Topic(topicName,this.client);            
        }

        public void createTopic(string topicName, int maxMsgSize)
        {   
            createTopic(topicName,maxMsgSize,1);
        }
        public void createTopic(string topicName, int maxMsgSize,int filterType =1 )
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (topicName == "" )
                throw new ClientException("Invalid parameter: topicName is empty");
            else
                param.Add("topicName", topicName);

            param.Add("filterType" , Convert.ToString(filterType));
            if(maxMsgSize<1 || maxMsgSize > 65536)
                throw new ClientException("Invalid paramter: maxMsgSize > 65536 or maxMsgSize < 1 ");

            string result = this.client.call("CreateTopic", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public void deleteTopic(string topicName)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (topicName == "" )
                throw new ClientException("Invalid parameter: topicName is empty");
            else
                param.Add("topicName", topicName);

            string result = this.client.call("DeleteTopic", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public void createSubscribe(string topicName, string subscriptionName, string endpoint, string protocol)
        {
            createSubscribe(topicName, subscriptionName, endpoint, protocol, null, null, "BACKOFF_RETRY", "JSON");
        }

        public void createSubscribe(string topicName, string subscriptionName, string endpoint, string protocol,
            List<string> filterTag,List<string> bindingKey, string notifyStrategy, string notifyContentFormat)
        {
            if (filterTag != null && filterTag.Count > 5)
                throw new ClientException("Invalid parameter:Tag number > 5");
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (topicName == "")
                throw new ClientException("Invalid parameter: topicName is empty");
            else
                param.Add("topicName", topicName);

            if (subscriptionName == "")
                throw new ClientException("Invalid parameter: subscriptionName is empty");
            else
                param.Add("subscriptionName", subscriptionName);
            if (endpoint == "")
                throw new ClientException("Invalid parameter: endpoint is empty");
            else
                param.Add("endpoint", endpoint);

            if (protocol == "")
                throw new ClientException("Invalid parameter: protocol is empty");
            else
                param.Add("protocol", protocol);

            if (notifyStrategy == "")
                throw new ClientException("Invalid parameter: notifyStrategy is empty");
            else
                param.Add("notifyStrategy", notifyStrategy);
            if (notifyContentFormat == "")
                throw new ClientException("Invalid parameter: notifyContentFormat is empty");
            else
                param.Add("notifyContentFormat", notifyContentFormat);

            if(filterTag != null)
            {
                for (int i = 0; i < filterTag.Count; ++i)
                    param.Add("filterTag." + Convert.ToString(i), filterTag[i]);
            }

            if (bindingKey != null)
            {
                for (int i = 0; i < bindingKey.Count; ++i)
                    param.Add("bindingKey." + Convert.ToString(i), bindingKey[i]);
            }

            string result = this.client.call("Subscribe", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }
        public void deleteSubscribe(string topicName, string subscriptionName)
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            if (topicName == "")
                throw new ClientException("Invalid parameter: topicName is empty");
            else
                param.Add("topicName", topicName);
            if (subscriptionName == "")
                throw new ClientException("Invalid parameter: subscriptionName is empty");
            else
                param.Add("subscriptionName", subscriptionName);

            string result = this.client.call("Unsubscribe", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public Subscription getSubscribe(string topicName, string subscriptionName)
        {
            return new Subscription(topicName, subscriptionName, this.client);
        }

    }

}