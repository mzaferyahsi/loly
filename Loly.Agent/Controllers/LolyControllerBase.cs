using Loly.Agent.ErrorResults;
using Loly.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace Loly.Agent.Controllers
{
    public abstract class LolyControllerBase : ControllerBase
    {
        [NonAction]
        public virtual InternalServerErrorResult InternalServerErrorResult(string code, string message,
            Severity severity)
        {
            return new InternalServerErrorResult(code, message, severity);
        }
    }
}