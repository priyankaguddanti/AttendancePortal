using System.Net;

namespace AttendancePortal.Code
{
    public class Result<T> : Result
    {
        public virtual T Value { get; set; }

        public Result(HttpStatusCode statusCode, string message = null)
          : base(statusCode, message)
        {
        }

        public Result(T value)
        {
            this.Value = value;
        }

        public Result(Result result)
        {
            this.Message = result.Message;
            this.Status = result.Status;
        }

        public Result ToResult()
        {
            return new Result(this.Status, this.Message);
        }
    }
}

