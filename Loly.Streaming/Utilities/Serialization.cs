using System;
using Confluent.Kafka;

namespace Loly.Streaming.Utilities
{
    public static  class Serialization
    {
        public static bool KafkaCanDeserialize(Type type)
        {
            return KafkaCanSerialize(type);
        }
        
        public static bool KafkaCanSerialize(Type type)
        {
            if (type == typeof(Null) ||
                type == typeof(Ignore) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(string) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(byte[]))
                return true;

            return false;
        }
    }
}