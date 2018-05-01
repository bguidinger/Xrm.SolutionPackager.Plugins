namespace SolutionPackager.Plugins
{
    using Microsoft.Crm.Tools.SolutionPackager;

    public class CustomControls : IPackagePlugin
    {
        public void BeforeRead(PluginContext pluginContext)
        {
            pluginContext.Context.AddProcessor<CustomControlsProcessor>();
            pluginContext.Context.AddRootComponent(ComponentType.CustomControl);
        }

        public void AfterRead(PluginContext pluginContext) { }

        public void BeforeWrite(PluginContext pluginContext) { }

        public void AfterWrite(PluginContext pluginContext) { }
    }
}