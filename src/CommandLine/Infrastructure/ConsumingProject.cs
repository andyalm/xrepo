using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace XRepo.CommandLine.Infrastructure
{
    public class ConsumingProject
    {
        public static ConsumingProject Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return new ConsumingProject(XDocument.Load(stream), filePath);
            }
        }

        private readonly XDocument _doc;
        public string FilePath { get; }

        public ConsumingProject(XDocument doc, string filePath)
        {
            _doc = doc;
            FilePath = filePath;
        }

        public void AddProjectReference(string projectPath)
        {
            var xrepoReferences = _doc.Root.Elements()
                .SingleOrDefault(e => e.Name.LocalName == "ItemGroup" &&
                                      (string)e.Attribute("Label") == "XRepoReferences");
            if (xrepoReferences == null)
            {
                xrepoReferences = new XElement(_doc.Root.Name.Namespace + "ItemGroup");
                xrepoReferences.Add(new XAttribute("Label", "XRepoPinReferences"));
                _doc.Root.Add(xrepoReferences);
            }

            var projectReference = new XElement(_doc.Root.Name.Namespace + "ProjectReference");
            projectReference.Add(new XAttribute("Include", projectPath));
            xrepoReferences.Add(projectReference);
        }

        public void Save()
        {
            var writerSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true
            };
            using (var writer = XmlWriter.Create(File.Open(FilePath, FileMode.Create), writerSettings))
            {
                _doc.Save(writer);
            }
        }

        public bool ReferencesPackage(string packageId)
        {
            return _doc.Root.Descendants(_doc.Root.Name.Namespace + "PackageReference")
                .Any(e => packageId.Equals((string) e.Attribute("Include"), StringComparison.OrdinalIgnoreCase));
        }

        public bool RemovePinProjectReferences()
        {
            var pinReferences = _doc.Root.Elements(_doc.Root.Name.Namespace + "ItemGroup")
                .SingleOrDefault(e => (string) e.Attribute("Label") == "XRepoPinReferences");

            if(pinReferences != null)
            {
                pinReferences.Remove();
                return true;
            }

            return false;
        }
    }
}