using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;

namespace Loly.Agent.Api.Controllers
{
    public class LolyControllerBase : ControllerBase
    {
        [NonAction]
        public virtual ObjectResult InternalServerErrorResult()
        {
            return new ObjectResult(new InternalServerError());
        }
        
        [NonAction]
        public virtual ObjectResult InternalServerErrorResult(string message)
        {
            return new ObjectResult(new InternalServerError(message));
        }
        
        [NonAction]
        public virtual ObjectResult NotFoundResult()
        {
            return new ObjectResult(new NotFound());
        }
        
        [NonAction]
        public virtual ObjectResult NotFoundResult(string message)
        {
            return new ObjectResult(new NotFound(message));
        }
    }
}