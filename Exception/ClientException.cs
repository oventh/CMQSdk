using System;
using System.Collections.Generic;
using System.Text;

namespace Cmq_SDK.Exception
{
    public class ClientException : ApplicationException
    {
        public ClientException(string message) : base(message) { }

        public override string Message
        {
            get { return base.Message; }
           
        }
    }
}
