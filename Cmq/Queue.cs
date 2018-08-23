using System;
using System.Collections.Generic;
using System.Text;
using Cmq_SDK.Cmq;
using Newtonsoft.Json.Linq;
using Cmq_SDK.Exception;
namespace Cmq_SDK.Cmq
{
    public class Queue
    {
        private string queueName;
        private CmqClient client;
        internal Queue(string queueName, CmqClient client) {
            this.queueName = queueName;
            this.client = client;
        }

        public void setQueueAttributes(QueueMeta meta) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            if (meta.maxMsgHeapNum > 0)
                param.Add("maxMsgHeapNum", Convert.ToString(meta.maxMsgHeapNum));
            if (meta.pollingWaitSeconds > 0)
                param.Add("pollingWaitSeconds", Convert.ToString(meta.pollingWaitSeconds));
            if (meta.visibilityTimeout > 0)
                param.Add("visibilityTimeout",Convert.ToString(meta.visibilityTimeout));
            if (meta.maxMsgSize > 0)
                param.Add("maxMsgSize", Convert.ToString(meta.maxMsgSize));
            if (meta.msgRetentionSeconds > 0)
                param.Add("msgRetentionSeconds", Convert.ToString(meta.msgRetentionSeconds));

            string result = this.client.call("SetQueueAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public QueueMeta getQueueAttributes() {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);

            string result = this.client.call("GetQueueAttributes", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            QueueMeta meta = new QueueMeta();
            meta.maxMsgHeapNum = (int)jObj["maxMsgHeapNum"];
            meta.pollingWaitSeconds = (int)jObj["pollingWaitSeconds"];
            meta.visibilityTimeout = (int)jObj["visibilityTimeout"];
            meta.maxMsgSize = (int)jObj["maxMsgSize"];
            meta.msgRetentionSeconds = (int)jObj["msgRetentionSeconds"];
            meta.createTime = (int)jObj["createTime"];
            meta.lastModifyTime = (int)jObj["lastModifyTime"];
            meta.activeMsgNum = (int)jObj["activeMsgNum"];
            meta.inactiveMsgNum = (int)jObj["inactiveMsgNum"];
            meta.rewindmsgNum = (int)jObj["rewindMsgNum"];
            meta.minMsgTime = (int)jObj["minMsgTime"];
            meta.delayMsgNum = (int)jObj["delayMsgNum"];

            return meta;
        }

        public string sendMessage(string msgBody) {
            return sendMessage(msgBody, 0);
        }
        public string sendMessage(string msgBody, int delayTime) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            param.Add("msgBody", msgBody);
            param.Add("delaySeconds", Convert.ToString(delayTime));

            string result = this.client.call("SendMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return jObj["msgId"].ToString();
        }

        public List<string> batchSendMessage(List<string> vtMsgBody, int delayTime) {
            if (vtMsgBody.Count == 0 || vtMsgBody.Count > 16)
                throw new ClientException("Error: message size is empty or more than 16");
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            for (int i = 0; i < vtMsgBody.Count; i++) {
                string k = "msgBody." + Convert.ToString(i + 1);
                param.Add(k, vtMsgBody[i]);
            }
            param.Add("delaySeconds", Convert.ToString(delayTime));

            string result = this.client.call("BatchSendMessage", param);

            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            List<string> vtMsgId = new List<string>();
            JArray idsArray = JArray.Parse(jObj["msgList"].ToString());
            foreach (var item in idsArray)
            {
                vtMsgId.Add(item["msgId"].ToString());
            }
            return vtMsgId;

        }

        public Message receiveMessage(int pollingWaitSeconds) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            if (pollingWaitSeconds > 0)
            {
                param.Add("UserpollingWaitSeconds", Convert.ToString(pollingWaitSeconds));
                param.Add("pollingWaitSeconds", Convert.ToString(pollingWaitSeconds));
            }
            else {
                param.Add("UserpollingWaitSeconds", Convert.ToString(30000));
            }
            string result = this.client.call("ReceiveMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());

            Message msg = new Message();
            msg.msgId = jObj["msgId"].ToString();
            msg.receiptHandle = jObj["receiptHandle"].ToString();
            msg.msgBody = jObj["msgBody"].ToString();
            msg.enqueueTime = (long)jObj["enqueueTime"];
            msg.nextVisibleTime = (long)jObj["nextVisibleTime"];
            msg.firstDequeueTime = (long)jObj["firstDequeueTime"];
            msg.dequeueCount = (int)jObj["dequeueCount"];
            return msg;
        }

        public List<Message> batchReceiveMessage(int numOfMsg, int pollingWaitSeconds) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            param.Add("numOfMsg", Convert.ToString(numOfMsg));
            if (pollingWaitSeconds > 0)
            {
                param.Add("UserpollingWaitSeconds", Convert.ToString(pollingWaitSeconds));
                param.Add("pollingWaitSeconds", Convert.ToString(pollingWaitSeconds));
            }
            else
            {
                param.Add("UserpollingWaitSeconds", Convert.ToString(30000)); 
            }
            string result = this.client.call("BatchReceiveMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());

            List<Message> vtMsg = new List<Message>();
            JArray idsArray = JArray.Parse(jObj["msgInfoList"].ToString());
            foreach (var item in idsArray)
            {
                Message msg = new Message();
                msg.msgId = item["msgId"].ToString();
                msg.receiptHandle = item["receiptHandle"].ToString();
                msg.msgBody = item["msgBody"].ToString();
                msg.enqueueTime = (long)item["enqueueTime"];
                msg.nextVisibleTime = (long)item["nextVisibleTime"];
                msg.firstDequeueTime = (long)item["firstDequeueTime"];
                msg.dequeueCount = (int)item["dequeueCount"];
                vtMsg.Add(msg);
            }
            return vtMsg;
        }

        public void deleteMessage(string receiptHandle) {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            param.Add("receiptHandle", receiptHandle);

            string result = this.client.call("DeleteMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }

        public void batchDeleteMessage(List<string> vtReceiptHandle) {
            if (vtReceiptHandle.Count == 0)
                return;
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            param.Add("queueName", this.queueName);
            for (int i = 0; i < vtReceiptHandle.Count; i++) {
                string k = "receiptHandle." + Convert.ToString(i+1);
                param.Add(k, vtReceiptHandle[i]);
            }

            string result = this.client.call("BatchDeleteMessage", param);
            JObject jObj = JObject.Parse(result);
            int code = (int)jObj["code"];
            if (code != 0)
                throw new ServerException(code, jObj["message"].ToString(), jObj["requestId"].ToString());
            return;
        }
    }
}
