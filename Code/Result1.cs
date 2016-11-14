using System.Net;
using System.Runtime.Serialization;

namespace AttendancePortal.Code
{
    public class Result
    {
        [DataMember(EmitDefaultValue = false)]
        public HttpStatusCode Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Message { get; set; }

        public bool HasError
        {
            get
            {
                if (this.Status >= HttpStatusCode.OK)
                    return this.Status > HttpStatusCode.NotModified;
                return true;
            }
        }

        public bool IsSuccess => !this.HasError;

        public Result()
        {
            this.Status = HttpStatusCode.OK;
        }

        public Result(HttpStatusCode statusCode, string message = null)
        {
            this.Status = statusCode;
            this.Message = message;
        }

        public static Result Success()
        {
            return new Result();
        }
    }
}