namespace SolutionPackager.Plugins
{
    using Microsoft.Crm.Tools;
    using Microsoft.Crm.Tools.SolutionPackager;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public class CustomControlsProcessor : IComponentProcessor
    {
        public string SupportedElementName => "CustomControls";

        public ComponentType SupportedComponentType => ComponentType.CustomControl;

        public bool IsDifferentInManaged => false;

        public string ParentTagForNestedMultiLcids { get; set; }
        public string ChildTagForNestedMultiLcids { get; set; }

        private Context context;

        public void Initialize(Context context)
        {
            this.context = context;
        }

        public ComponentCollection CreateComponents(XElement root)
        {
            var components = new ComponentCollection(SupportedComponentType, root);

            foreach (var element in root.Elements())
            {
                components.Add(new Component()
                {
                    Id = new Guid(),
                    Element = element,
                    ComponentType = SupportedComponentType
                });
            }

            return components;
        }

        public ComponentCollection ReadFromFiles()
        {
            var components = new ComponentCollection(ComponentType.CustomControl, new XElement(SupportedElementName));

            var directory = Path.Combine(context.RootFolder, "Controls");
            var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllBytes(file);
                var path = file.Replace(context.RootFolder, "");
                context.Customizations.AddComponentFile(new Uri(path, UriKind.Relative), content);

                if (file.Contains("ControlManifest.xml"))
                {
                    var schemaName = GetSchemaName(content);
                    Logger.Message(TraceLevel.Info, $" - {schemaName}");
                    components.Add(new Component()
                    {
                        Id = new Guid(),
                        Element = null,
                        PrimaryName = schemaName,
                        ComponentType = ComponentType.CustomControl
                    });
                }

            }
            return components;
        }

        public void WriteToFiles(ComponentCollection components)
        {
            var document = context.Customizations.CustomizationsXDocument;
            var controls = document.Root.XPathSelectElement(SupportedElementName);
            controls.ReplaceWith(components.Element);

            // Write files to disk.
            var helper = context.GetType().Assembly.GetType("Microsoft.Crm.Tools.SolutionPackager.Helper");
            var writeToFile = helper.GetMethod("WriteToFile", new Type[] { typeof(string), typeof(byte[]) });

            var files = context.Customizations.ComponentFiles.Where(x => x.Key.StartsWith("/Controls/"));
            foreach (var file in files)
            {
                var path = Path.Combine(context.RootFolder, file.Value.FileName.TrimStart('/'));
                if (path.Contains("ControlManifest.xml"))
                {
                    var schemaName = GetSchemaName(file.Value.Bytes);
                    Logger.Message(TraceLevel.Info, $" - {schemaName}");
                }

                writeToFile.Invoke(null, new object[] { path, file.Value.Bytes });
            }
        }

        public Collection<LocalizableElement> GetLocalizableElements(ComponentCollection components)
        {
            return null;
        }

        private string GetSchemaName(byte[] manifestBytes)
        {
            using (var stream = new MemoryStream(manifestBytes))
            {
                var manifest = XDocument.Load(stream);
                var control = manifest.Root.Element("control");
                return $"{control.Attribute("namespace")?.Value}.{control.Attribute("constructor")?.Value}";
            }
        }
    }
}