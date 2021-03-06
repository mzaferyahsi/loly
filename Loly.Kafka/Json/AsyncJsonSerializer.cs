using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Loly.Kafka.Json
{
    public class AsyncJsonSerializer<T> : IAsyncSerializer<T>
    {
        public Task<byte[]> SerializeAsync(T data, SerializationContext context)
        {
            return new Task<byte[]>(() =>
            {
                var serializedObject = JsonConvert.SerializeObject(data);
                return Encoding.ASCII.GetBytes(serializedObject);
            });
        }
    }
}