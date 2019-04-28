namespace Loly.Agent.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }

        public KafkaConsumerConfig Consumer { get; set; }
    }

    public class KafkaConsumerConfig
    {
        public string GroupId { get; set; }
    }
}