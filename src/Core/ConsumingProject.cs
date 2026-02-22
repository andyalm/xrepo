using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace XRepo.Core
{
    public class ConsumingProject
    {
        private const string XRepoReferenceLabel = "XRepoReference";

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
            var ns = _doc.Root!.Name.Namespace;
            var xrepoReferences = _doc.Root.Elements()
                .SingleOrDefault(e => e.Name.LocalName == "ItemGroup" &&
                                      (string?)e.Attribute("Label") == XRepoReferenceLabel);
            if (xrepoReferences == null)
            {
                xrepoReferences = new XElement(ns + "ItemGroup");
                xrepoReferences.Add(new XAttribute("Label", XRepoReferenceLabel));
                _doc.Root.Add(xrepoReferences);
            }

            var alreadyReferenced = xrepoReferences.Elements(ns + "ProjectReference")
                .Any(e => string.Equals((string?)e.Attribute("Include"), projectPath,
                    StringComparison.OrdinalIgnoreCase));
            if (!alreadyReferenced)
            {
                var projectReference = new XElement(ns + "ProjectReference");
                projectReference.Add(new XAttribute("Include", projectPath));
                xrepoReferences.Add(projectReference);
            }
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
            return _doc.Root!.Descendants(_doc.Root.Name.Namespace + "PackageReference")
                .Any(e => packageId.Equals((string?)e.Attribute("Include"), StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveXRepoProjectReferences()
        {
            var linkedReferences = _doc.Root!.Elements(_doc.Root.Name.Namespace + "ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == XRepoReferenceLabel).ToArray();

            if (linkedReferences.Length > 0)
            {
                foreach (var el in linkedReferences)
                    el.Remove();
                return true;
            }

            return false;
        }

        public bool RemoveXRepoProjectReference(string projectPath)
        {
            var ns = _doc.Root!.Name.Namespace;
            var xrepoItemGroups = _doc.Root.Elements(ns + "ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == XRepoReferenceLabel).ToArray();

            bool removed = false;
            foreach (var itemGroup in xrepoItemGroups)
            {
                var matchingRefs = itemGroup.Elements(ns + "ProjectReference")
                    .Where(e => string.Equals((string?)e.Attribute("Include"), projectPath,
                        StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                foreach (var refElement in matchingRefs)
                {
                    refElement.Remove();
                    removed = true;
                }

                if (!itemGroup.HasElements)
                    itemGroup.Remove();
            }

            return removed;
        }

        public bool HasXRepoProjectReferences()
        {
            var ns = _doc.Root!.Name.Namespace;
            return _doc.Root.Elements(ns + "ItemGroup")
                .Any(e => (string?)e.Attribute("Label") == XRepoReferenceLabel);
        }
    }
}
