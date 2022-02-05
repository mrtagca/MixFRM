using System;
using System.Collections.Generic;
using System.Text;

namespace MixFRM.BaseTypes
{
    public class ResponseBase
    {
        public bool Success { get; set; }
        public string RecordId { get; set; }
        public Error Error { get; set; }
    }

    public class ResponseData<T> : ResponseBase
    {
        public T Data { get; set; }
    }

    public class ResponseList<T> : ResponseBase
    {
        public List<T> Data { get; set; }
    }

    public class Error
    {

        public Error(Exception ex)
        {
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;
        }

        public Error(string errorMessage)
        {
            this.Message = errorMessage;
        }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
