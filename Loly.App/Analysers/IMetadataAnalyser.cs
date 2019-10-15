using System.Threading.Tasks;
using Loly.Streaming.Models;

namespace Loly.App.Analysers
{
    public interface IMetadataAnalyser
    {
        Task Analyse(FileMetaData metaDataMessage);
    }
}