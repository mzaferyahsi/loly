using System;
using Loly.Agent.Configuration;
using Loly.Agent.Controllers;
using Loly.Agent.Discovery;
using Loly.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DiscoveriesController : LolyControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger _log;
        private LolyAgentFeatureManager _featureManager;

        public DiscoveriesController(IDiscoveryService service, LolyAgentFeatureManager featureManager, ILogger<DiscoveriesController> logger)
        {
            _log = logger;
            _discoveryService = service;
            _featureManager = featureManager;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult Post(Models.Api.Discovery discovery)
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
                _log.LogError(e, "Unable to start discovery.");
                return InternalServerErrorResult("POSTDISC01", $"Unable to create discovery for path {discovery.Path}",
                    Severity.Error);
            }
        }
    }
}