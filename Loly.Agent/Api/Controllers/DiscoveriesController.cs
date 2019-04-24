using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;

namespace Loly.Agent.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class DiscoveriesController : LolyControllerBase
    {
        private ILog _log = LogManager.GetLogger(typeof(DiscoveriesController));
        private Discoveries.DiscoveriesController _discoveriesController;

        public Discoveries.DiscoveriesController Controller => _discoveriesController ?? (_discoveriesController = new Discoveries.DiscoveriesController());


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult Post(Discovery discovery)
        {
            try
            {
                Controller.Discover(discovery);
                return Created(string.Empty, new { });
            }
            catch (Exception e)
            {
                _log.Error(e);
                return InternalServerErrorResult("Unable to create discovery");
            }
            
        }
    }
}