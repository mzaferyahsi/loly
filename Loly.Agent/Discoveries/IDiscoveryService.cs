using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loly.Agent.Models;
using Loly.Agent.Services;

namespace Loly.Agent.Discovery
{
    public interface IDiscoveryService : IService
    {
        Task GetDiscoverTask(string path);

        Task GetDiscoverTask(string path, IList<string> exclusions);
        
        void Discover(string path);
        
        void Discover(string path, IList<string> exclusions);
    }
}