using System;
using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Loly.Kafka.Json
{
    public class JsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull)
                return default(T);

            var objectString = Encoding.ASCII.GetString(data.ToArray());
            return JsonConvert.DeserializeObject<T>(objectString);
        }
    }
}