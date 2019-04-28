using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Hangfire;
using log4net;
using Loly.Agent.Controllers;
using Loly.Agent.Discovery;
using Microsoft.AspNetCore.Http;

namespace Loly.Agent.Api
{

    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DiscoveriesController : LolyControllerBase
    {
        private ILog _log = LogManager.GetLogger(typeof(DiscoveriesController));
        private IDiscoveryService _discoveryService;
        public DiscoveriesController(IDiscoveryService service)
        {
            this._discoveryService = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult Post(Models.Discovery discovery)
        {
            try
            {
                _discoveryService.GetDiscoverTask(discovery.Path).Start();
                return Created(string.Empty, new { });
            }
            catch (Exception e)
            {
                _log.Error(e);
                return InternalServerErrorResult("POSTDISC01", $"Unable to create discovery for path {discovery.Path}", Severity.Error);
            }
            
        }
    }
}