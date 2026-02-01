using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SequenceSystem.Domain
{
    public class ProcessorFactory
    {
        // Cache the types to keep Reflection fast
        private readonly Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        public void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<ProcessorAttribute>() != null);

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<ProcessorAttribute>();
                    _typeMap[attr.DataType] = type;
                }
            }
        }

        public ITaskProcessor CreateProcessor(object nodeData)
        {
            var dataType = nodeData.GetType();
            if (_typeMap.TryGetValue(dataType, out var processorType))
            {
                // Option A: New instance every time
                return (ITaskProcessor)Activator.CreateInstance(processorType, nodeData);
            }
            return null;
        }
    }
}