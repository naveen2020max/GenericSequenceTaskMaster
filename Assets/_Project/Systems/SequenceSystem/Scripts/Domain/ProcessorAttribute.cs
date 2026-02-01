using System;

namespace SequenceSystem.Domain
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessorAttribute : Attribute
    {
        public Type DataType { get; }
        public ProcessorAttribute(Type dataType) => DataType = dataType;
    }
}