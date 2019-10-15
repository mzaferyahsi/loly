using Microsoft.Extensions.Options;

namespace Loly.Configuration.Agent
{
    public class LolyAgentFeatureManager
    {
        private LolyAgentFeatureConfiguration _configuration;
        
        public LolyAgentFeatureManager(IOptions<LolyAgentFeatureConfiguration> configuration)
        {
            if (configuration?.Value == null)
                _configuration = new LolyAgentFeatureConfiguration()
                {
                    Discover = true,
                    AnalyseFile = true,
                    AnalyseFileHash = true,
                    AnalyseImageMetadata = true
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

        public bool IsImageMetadataAnalyserEnabled()
        {
            return _configuration.AnalyseImageMetadata;
        }
    }
}