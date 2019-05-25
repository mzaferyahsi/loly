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
    public class DiscoveryService : IDiscoveryService, IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(DiscoveryService));
        private readonly IKafkaProducerQueue _kafkaProducerQueue;
        private readonly KafkaProducerHostedService _kafkaProducerHostedService;

        public DiscoveryService(IKafkaConfigProducer configProducer)
        {
            _kafkaProducerQueue = new KafkaProducerQueue();
            _kafkaProducerHostedService = new KafkaProducerHostedService(configProducer, _kafkaProducerQueue);
            _kafkaProducerHostedService.StartAsync(CancellationToken.None);
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

            _kafkaProducerQueue.Enqueue(message);
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

                foreach (var _path in paths)
                {
                    Discover(_path);
                }

//                Parallel.ForEach(paths, Discover);
            }
            catch (UnauthorizedAccessException e)
            {
                _log.WarnFormat("Unable to access {0} due to authorization error", path);
            }
        }

        public void Dispose()
        {
            _kafkaProducerHostedService.StopAsync(CancellationToken.None);
            _kafkaProducerHostedService?.Dispose();
        }
    }
}