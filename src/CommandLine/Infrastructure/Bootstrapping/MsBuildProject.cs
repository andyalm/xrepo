using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XRepo.CommandLine.Infrastructure.Bootstrapping
{
    class MsBuildProject
    {
        private readonly string _path;

        public MsBuildProject(string path)
        {
            _path = path;
        }

        public void AddExtensionImport(string importPath)
        {
            var project = Load();
            
            var xmlns = project.Root.Name.Namespace;
            var import = new XElement(xmlns + "Import");
            import.SetAttributeValue("Project", importPath);
            import.SetAttributeValue("Condition", $"Exists('{importPath}') and $(DisableGlobalXRepo)!='true'");
            project.Root.Add(import);
            
            Save(project);
        }

        public void RemoveImport(string projectPath)
        {
            var project = Load();
            var xmlns = project.Root.Name.Namespace;

            var importElement = project.Root.Elements(xmlns + "Import")
                .FirstOrDefault(e => ((string)e.Attribute("Project")).Equals(projectPath, StringComparison.OrdinalIgnoreCase));
            
            importElement.Remove();
        }

        public void RemoveImportsMatching(string projectPathSubstring)
        {
            var project = Load();
            var xmlns = project.Root.Name.Namespace;

            var importsToRemove = project.Root.Elements(xmlns + "Import")
                .Where(e => ((string)e.Attribute("Project")).ToLowerInvariant().Contains(projectPathSubstring.ToLowerInvariant()));
            
            foreach(var importToRemove in importsToRemove)
            {
                importToRemove.Remove();
            }
        }

        public IEnumerable<string> Imports
        {
            get
            {
                var project = Load();
                var xmlns = project.Root.Name.Namespace;
                return project.Root.Elements(xmlns + "Import").Select(e => (string)e.Attribute("Project"));
            }
        }

        private XDocument Load()
        {
            using(var stream = File.OpenRead(_path))
            {
                return XDocument.Load(stream);
            }
        }

        private void Save(XDocument doc)
        {
            using(var stream = File.Open(_path, FileMode.Create))
            {
                doc.Save(stream);
            }
        }
    }
}