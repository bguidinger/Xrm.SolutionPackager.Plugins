namespace SolutionPackager.Plugins
{
    using Microsoft.Crm.Tools.SolutionPackager;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class Extensions
    {
        public static void AddProcessor<TProcessor>(this Context context) where TProcessor : IComponentProcessor, new()
        {
            var processor = new TProcessor();
            processor.Initialize(context);

            var contextType = context.GetType();

            var processorsField = contextType.GetPrivateField("componentProcessors");
            var processors = processorsField.GetValue(context) as List<IComponentProcessor>;
            processors.Add(processor);
            processorsField.SetValue(context, processors);

            var processorTypeDictionaryField = contextType.GetPrivateField("processorTypeDictionary");
            var processorTypeDictionary = processorTypeDictionaryField.GetValue(context) as IDictionary<ComponentType, IComponentProcessor>;
            processorTypeDictionary.Add(processor.SupportedComponentType, processor);
            processorTypeDictionaryField.SetValue(context, processorTypeDictionary);

            var processorElementNameDictionaryField = contextType.GetPrivateField("processorElementNameDictionary");
            var processorElementNameDictionary = processorElementNameDictionaryField.GetValue(context) as IDictionary<string, IComponentProcessor>;
            processorElementNameDictionary.Add(processor.SupportedElementName, processor);
            processorElementNameDictionaryField.SetValue(context, processorElementNameDictionary);
        }

        public static void AddRootComponent(this Context context, ComponentType type)
        {
            var contextType = context.GetType();
            var plugin = contextType.Assembly.GetType("Microsoft.Crm.Tools.SolutionPackager.Plugins.RootComponentsValidation");
            var typesField = plugin.GetPrivateField("RootComponentTypes");
            var types = typesField.GetValue(plugin) as IEnumerable<ComponentType>;
            var typeList = types.ToList();
            typeList.Add(type);
            typesField.SetValue(plugin, typeList.ToArray());

        }

        private static FieldInfo GetPrivateField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
        }
    }
}
