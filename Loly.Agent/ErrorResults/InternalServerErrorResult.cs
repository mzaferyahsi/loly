using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;

namespace Loly.Agent.ErrorResults
{
    public class InternalServerErrorResult : ObjectResult
    {
        public InternalServerErrorResult(string code, string message, Severity severity) : base(new Error()
            {Code = code, Message = message, Severity = severity})
        {
            this.StatusCode = 500;
        }
    }
}