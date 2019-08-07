using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loly.Agent.Discovery;
using Loly.Agent.Utility;
using Loly.Kafka;

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
        
        public virtual Task GetDiscoverTask(string path, IList<string> exclusions)
        {
            var task = new Task(() => Discover(path, exclusions));

            return task;
        }

        public void Discover(string path)
        {
            Discover(path, 
                new List<string>());
        }

        public void Discover(string path, IList<string> exclusions)
        {
//            _log.DebugFormat("Received {0} for discovery.", path);
            try
            {
                var homePathExclusions = exclusions.Where(x => x.StartsWith("~"));

                foreach (var homePathExclusion in homePathExclusions)
                {
                    exclusions.Remove(homePathExclusion);
                    exclusions.Add(PathResolver.Resolve(homePathExclusion));
                }
                
                path = PathResolver.Resolve(path);
                
                foreach (var exclusion in exclusions)
                    if (path.StartsWith(exclusion))
                        return;

                var fileAttr = File.GetAttributes(Path.GetFullPath(path));

                if ((fileAttr & FileAttributes.Directory) != 0)
                {
                    DiscoverDirectory(Path.GetFullPath(path), exclusions);
                }
                else
                {
                    QueueMessage(Path.GetFullPath(path));
                }
            }
            catch (FileNotFoundException)
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

        private void DiscoverDirectory(string path, IList<string> exclusions)
        {
            try
            {
                QueueMessage(path);

                var di = new DirectoryInfo(path);
                var files = di.GetFiles().Select(x => x.FullName);
                var directories = di.GetDirectories().Select(x => x.FullName);

                foreach (var file in files)
                {
                    Discover(file, exclusions);
                }

                foreach (var directory in directories)
                {
                    Discover(directory, exclusions);
                }

//                var paths = files.Concat(directories).ToArray();
//                Parallel.ForEach(paths, Discover);
            }
            catch (UnauthorizedAccessException e)
            {
                _log.WarnFormat("Unable to access {0} due to authorization error", path);
                _log.Warn(e);
            }
        }

        public void Dispose()
        {
            _kafkaProducerHostedService.StopAsync(CancellationToken.None);
            _kafkaProducerHostedService?.Dispose();
        }
    }
}