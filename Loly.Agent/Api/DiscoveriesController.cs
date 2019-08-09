using System;
using log4net;
using Loly.Agent.Configuration;
using Loly.Agent.Controllers;
using Loly.Agent.Discovery;
using Loly.Agent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Loly.Agent.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DiscoveriesController : LolyControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILog _log = LogManager.GetLogger(typeof(DiscoveriesController));
        private LolyFeatureManager _featureManager;

        public DiscoveriesController(IDiscoveryService service, LolyFeatureManager featureManager)
        {
            _discoveryService = service;
            _featureManager = featureManager;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult Post(Models.Discovery discovery)
        {
            if (!_featureManager.IsDiscoverEnabled())
            {
                return InternalServerErrorResult("POSTDISCDIS01", "Discovery feature is disabled.", Severity.Error);
            }
            
            try
            {
                var task = _discoveryService.GetDiscoverTask(discovery.Path, discovery.Exclusions);
                task.Start();
                return Created(string.Empty, new { });
            }
            catch (Exception e)
            {
                _log.Error(e);
                return InternalServerErrorResult("POSTDISC01", $"Unable to create discovery for path {discovery.Path}",
                    Severity.Error);
            }
        }
    }
}