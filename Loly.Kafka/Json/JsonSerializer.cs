using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Loly.Kafka.Json
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            var serializedObject = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(serializedObject);
        }
    }
}