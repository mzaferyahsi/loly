using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;
using Loly.Agent.Discovery;
using Loly.Agent.Kafka;
using Loly.Analysers.Utility;
using Loly.Kafka.Config;
using Loly.Kafka.Models;
using Loly.Kafka.Producer;

namespace Loly.Agent.Discoveries
{
    public class DiscoveryService : IDiscoveryService, IDisposable
    {
        private readonly IProducerService<string, string> _kafkaProducerService;
        private readonly IProducerQueue<string, string> _kafkaProducerQueue;
        private readonly ILog _log = LogManager.GetLogger(typeof(DiscoveryService));

        public DiscoveryService(IConfigProducer configProducer)
        {
            _kafkaProducerService = new ProducerService<string, string>(configProducer, _log);
            _kafkaProducerQueue = _kafkaProducerService.Queue;
            _kafkaProducerService.Start(CancellationToken.None);
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
                var fullPath = Path.GetFullPath(path);

                ResolveExclusions(exclusions);

                foreach (var exclusion in exclusions)
                {
                    var shouldExclude = Regex.IsMatch(fullPath, exclusion, RegexOptions.IgnoreCase);
                    if (shouldExclude)
                    {
                        _log.Debug($"Skipping ${fullPath} because it matches ${exclusion} as exclusion filter.");
                        return;
                    }
                }

                var fileAttr = File.GetAttributes(fullPath);

                if ((fileAttr & FileAttributes.Directory) != 0)
                    DiscoverDirectory(fullPath, exclusions);
                else
                    QueueMessage(fullPath);
            }
            catch (FileNotFoundException)
            {
                _log.Warn($"{path} not found.");
            }
        }

        public void Dispose()
        {
            _kafkaProducerService.Stop(CancellationToken.None);
            _kafkaProducerService?.Dispose();
        }

        private static void ResolveExclusions(IList<string> exclusions)
        {
            if (exclusions.Any(x => x.Contains("~/")))
            {
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
            var message = new KafkaMessage<string, string>
            {
                Topic = "loly-discovered",
                Message = new Message<string, string>()
                {
                    Key = path,
                    Value = path
                }
            };

            _kafkaProducerQueue.Enqueue(message);
        }

        private void DiscoverDirectory(string path, IList<string> exclusions)
        {
            try
            {
                ResolveExclusions(exclusions);

                foreach (var exclusion in exclusions)
                {
                    var shouldExclude = Regex.IsMatch(path, exclusion, RegexOptions.IgnoreCase);
                    if (shouldExclude)
                    {
                        _log.Debug($"Skipping ${path} because it matches ${exclusion} as exclusion filter.");
                        return;
                    }
                }

                QueueMessage(path);

                var di = new DirectoryInfo(path);
                var files = di.GetFiles().Select(x => x.FullName).ToList();
                var directories = di.GetDirectories().Select(x => x.FullName).ToList();

                files.ForEach(file => { Discover(file, exclusions); });
                directories.ForEach(directory => { Discover(directory, exclusions); });
            }
            catch (UnauthorizedAccessException e)
            {
                _log.WarnFormat("Unable to access {0} due to authorization error", path);
                _log.Warn(e);
            }
        }
    }
}