using System;
using System.Collections.Generic;
using System.Text;

namespace Digitalist_Data.Helpers
{
    public enum ResultType
    {
        Success,
        Error
    }

    public interface IResult<T>
    {
        string Message { get; }
        T ReturnObj { get; }
        ResultType Type { get; }
    }

    public class Result<T> : IResult<T>
    {
        public string Message { get; set; }
        public T ReturnObj { get; set; }
        public ResultType Type { get; set; }

            
        public Result(string message, ResultType type)
        {
            Message = message;
            Type = type;
        }
        public Result(string message, ResultType type, T returnObj)
        {
            Message = message;
            ReturnObj = returnObj;
            Type = type;
        }
    }    
    
    public class Result: Result<object>
    {
        public Result(string message, ResultType type) : base(message, type)
        {

        }
    }
}
