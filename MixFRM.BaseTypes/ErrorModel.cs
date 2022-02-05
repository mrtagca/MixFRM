using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.BaseTypes
{
    public class ErrorModel
    {
        public ErrorModel(string message, string stackTrace)
        {
            Message = message;
            StackTrace = stackTrace;
        }

        public ErrorModel(Exception exp) : this(exp.Message, exp.StackTrace)
        { }
        public ErrorModel(string message) : this(message, null)
        { }

        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
