using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loly.Agent.Discovery;
using Loly.Agent.Kafka;

namespace Loly.Agent.Discoveries
{
    public class DiscoveryService : IDiscoveryService
    {
        private ILog _log = LogManager.GetLogger(typeof(DiscoveryService));
        private Queue<string> _queue = new Queue<string>();
        private IKafkaProducerHostedService _kafkaProducerHostedService;

        public DiscoveryService(IKafkaProducerHostedService kafkaProducerHostedService)
        {
            _kafkaProducerHostedService = kafkaProducerHostedService;
        }

        public virtual Task GetDiscoverTask(string path)
        {
            var task = new Task(() => Discover(path));

            return task;
        }

        public void Discover(string path)
        {
            _log.DebugFormat("Received {0} for discovery.", path);
            try
            {
                if (path.StartsWith("~/"))
                {
                    string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                       Environment.OSVersion.Platform == PlatformID.MacOSX)
                        ? Environment.GetEnvironmentVariable("HOME")
                        : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                    path = path.Replace("~", homePath);
                }

                var fileAttr = File.GetAttributes(Path.GetFullPath(path));

                if ((fileAttr & FileAttributes.Directory) != 0)
                {
                    DiscoverDirectory(Path.GetFullPath(path));
                }
                else
                {
                    QueueMessage(Path.GetFullPath(path));
                }
            }
            catch (FileNotFoundException e)
            {
                _log.Warn($"{path} not found.");
            }
        }

        private void QueueMessage(string path)
        {
            var message = new KafkaMessage()
            {
                Topic = "loly-discovered",
                Message = path
            };

            _kafkaProducerHostedService.AddMessage(message);
            _kafkaProducerHostedService.StartAsync(CancellationToken.None);
        }

        private void DiscoverDirectory(string path)
        {
            try
            {
                QueueMessage(path);

                var di = new DirectoryInfo(path);
                var files = di.GetFiles().Select(x => x.FullName);
                var directories = di.GetDirectories().Select(x => x.FullName);
                var paths = files.Concat(directories).ToArray();

                Parallel.ForEach(paths, Discover);
            }
            catch (UnauthorizedAccessException e)
            {
                _log.WarnFormat("Unable to access {0} due to authorization error", path);
            }
        }
    }
}