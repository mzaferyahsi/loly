namespace Loly.Agent.Kafka
{
    public class KafkaMessage
    {
        public string Topic { get; set; }
        public object Message { get; set; }
    }
}