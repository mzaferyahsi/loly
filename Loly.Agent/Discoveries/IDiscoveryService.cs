using System.Collections.Generic;
using System.Threading.Tasks;
using Loly.Agent.Services;

namespace Loly.Agent.Discoveries
{
    public interface IDiscoveryService : IService
    {
        Task GetDiscoverTask(string path);

        Task GetDiscoverTask(string path, IList<string> exclusions);

        void Discover(string path);

        void Discover(string path, IList<string> exclusions);
    }
}