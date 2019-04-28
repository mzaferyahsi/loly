using System.Threading.Tasks;
using Loly.Agent.Models;
using Loly.Agent.Services;

namespace Loly.Agent.Discovery
{
    public interface IDiscoveryService : IService
    {
        Task GetDiscoverTask(string path);

        void Discover(string path);
    }
}