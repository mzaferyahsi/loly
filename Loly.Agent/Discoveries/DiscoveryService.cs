using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Loly.Analysers.Utility;
using Loly.Streaming.Config;
using Loly.Streaming.Models;
using Loly.Streaming.Producer;
using Microsoft.Extensions.Logging;

namespace Loly.Agent.Discoveries
{
    public class DiscoveryService : IDiscoveryService, IDisposable
    {
        private readonly IProducerService<string, string> _kafkaProducerService;
        private readonly IProducerQueue<string, string> _kafkaProducerQueue;
        private readonly ILogger _logger;

        public DiscoveryService(IConfigProducer configProducer, ILogger<DiscoveryService> logger)
        {
            _logger = logger;
            _kafkaProducerService = new ProducerService<string, string>(configProducer, _logger);
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
                        _logger.LogDebug($"Skipping ${fullPath} because it matches ${exclusion} as exclusion filter.");
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
                _logger.LogWarning($"{path} not found.");
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
            var message = new StreamMessage<string, string>
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
                        _logger.LogDebug($"Skipping ${path} because it matches ${exclusion} as exclusion filter.");
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
                _logger.LogWarning(e, $"Unable to access {path} due to authorization error");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to discover directory {path}", e);
            }
        }
    }
}