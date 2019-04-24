using System;
using System.Threading.Tasks;
using log4net;
using Loly.Agent.Models;

namespace Loly.Agent.Discoveries
{
    public class DiscoveriesController
    {
        private ILog _log = LogManager.GetLogger(typeof(DiscoveriesController));
        
        public void Discover(Discovery discovery)
        {
            _log.DebugFormat("Received {0} for discovery.", discovery.Path);
        }
    }
}