using Microsoft.Extensions.Options;

namespace Loly.Agent.Configuration
{
    public class LolyFeatureManager
    {
        private LolyFeatureConfiguration _configuration;
        
        public LolyFeatureManager(IOptions<LolyFeatureConfiguration> configuration)
        {
            if (configuration == null || configuration.Value == null)
                _configuration = new LolyFeatureConfiguration()
                {
                    Discover = true,
                    AnalyseFile = true,
                    AnalyseFileHash = true
                };
            else
                _configuration = configuration.Value;
        }

        public bool IsDiscoverEnabled()
        {
            return _configuration.Discover;
        }

        public bool IsFileAnalyserEnabled()
        {
            return _configuration.AnalyseFile;
        }

        public bool IsFileHashAnalyserEnabled()
        {
            return _configuration.AnalyseFileHash;
        }
    }
}