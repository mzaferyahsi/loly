using System.Net;
using Newtonsoft.Json;

namespace Loly.Agent.Models
{
    public class ApiResult 
    {
        public int StatusCode { get; private set; }

        public string StatusDescription { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; private set; }

        public ApiResult(int statusCode, string statusDescription)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
        }

        public ApiResult(int statusCode, string statusDescription, string message)
            : this(statusCode, statusDescription)
        {
            this.Message = message;
        }
    }
    
    public class InternalServerError : ApiResult
    {
        public InternalServerError()
            : base(500, HttpStatusCode.InternalServerError.ToString())
        {
        }


        public InternalServerError(string message)
            : base(500, HttpStatusCode.InternalServerError.ToString(), message)
        {
        }
    }
    
    
    public class NotFound: ApiResult
    {
        public NotFound()
            : base(404, HttpStatusCode.NotFound.ToString())
        {
        }


        public NotFound(string message)
            : base(404, HttpStatusCode.NotFound.ToString(), message)
        {
        }
    }
}