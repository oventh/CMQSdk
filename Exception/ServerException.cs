using System;
using System.Collections.Generic;
using System.Text;

namespace Cmq_SDK.Exception
{
    public class ServerException : ApplicationException
    {
        private int httpStatus = 200;
        private int errorCode = 0;
        private String errorMessage = "";
        private String requestId = "";

        public ServerException(int httpStatus)  { this.httpStatus = httpStatus; }
        public ServerException(int errorCode, string errorMessage, string requestId) {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.requestId = requestId;
        }
        public override string ToString()
        {
            if (this.httpStatus != 200)
                return "http status: " + this.httpStatus;
            else
                return "code:" + this.errorCode
                    + ", message:" + this.errorMessage
                    + ", requestId" + this.requestId;
        }
    }
}
