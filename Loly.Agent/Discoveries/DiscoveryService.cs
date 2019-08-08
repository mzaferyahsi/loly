using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IKafkaProducerHostedService _kafkaProducerHostedService;

        public DiscoveryService(IKafkaProducerHostedService kafkaProducerHostedService)
        {
            _kafkaProducerHostedService = kafkaProducerHostedService;
            _kafkaProducerQueue = _kafkaProducerHostedService.Queue;
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
            try
            {
                path = PathResolver.Resolve(path);
                
                ResolveExclusions(exclusions);

                foreach (var exclusion in exclusions)
                {
                    var shouldExclude = Regex.IsMatch(path, exclusion, RegexOptions.IgnoreCase);
                    if(shouldExclude)
                        return;
                }
                
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

        private static void ResolveExclusions(IList<string> exclusions)
        {
            if(exclusions.Any(x=> x.Contains("~/"))) {
                
                var homePathExclusions = new List<string>();
                homePathExclusions.AddRange(exclusions.Where(x => x.Contains("~/")).ToArray());

                foreach (var homePathExclusion in homePathExclusions)
                {
                    exclusions.Remove(homePathExclusion);

                    exclusions.Add(homePathExclusion.Replace("~/", PathResolver.Resolve("~/")));
                }
                
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
                var files = di.GetFiles().Select(x => x.FullName).ToList();
                var directories = di.GetDirectories().Select(x => x.FullName).ToList();

                Parallel.ForEach(files, (file) => { Discover(file, exclusions); });

                Parallel.ForEach(directories, directory => { Discover(directory, exclusions); });

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