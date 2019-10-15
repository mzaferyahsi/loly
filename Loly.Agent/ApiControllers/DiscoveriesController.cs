using System;
using Loly.Agent.Discoveries;
using Loly.Configuration.Agent;
using Loly.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DiscoveriesController : LolyControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger _logger;
        private LolyAgentFeatureManager _featureManager;

        public DiscoveriesController(IDiscoveryService service, LolyAgentFeatureManager featureManager, ILogger<DiscoveriesController> logger)
        {
            _logger = logger;
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
                _logger.LogError(e, "Unable to start discovery.");
                return InternalServerErrorResult("POSTDISC01", $"Unable to create discovery for path {discovery.Path}",
                    Severity.Error);
            }
        }
    }
}