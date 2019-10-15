using System;

namespace Loly.Models.Attributes
{
    public class MetadataValueTypeAttribute : Attribute
    {
        public Type Value { get; }

        public MetadataValueTypeAttribute(Type type)
        {
            Value = type;
        }
    }
}