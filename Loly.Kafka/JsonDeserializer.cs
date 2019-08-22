using System;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net.ElasticSearch;
using Newtonsoft.Json;

namespace Loly.Kafka
{
    public class JsonDeserializer<T> : IAsyncDeserializer<T>
    {
        public Task<T> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
        {
            return new Task<T>(() =>
            {
                if (isNull)
                    return default(T);

                var objectString = Encoding.ASCII.GetString(data.ToArray());
                return JsonConvert.DeserializeObject<T>(objectString);
            });
        }
    }
}