using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XRepo.Core
{
    /// <summary>
    /// Represents a project that is consuming one or more projects managed by xrepo.
    /// </summary>
    /// <remarks>
    /// This class does not use an xml parser to serialize the changes to avoid introducing potentially unrelated
    /// whitespace changes, which can result in a dirty git status even after running the unref command. The goal is to
    /// avoid leaving a dirty git status after we're done.
    /// </remarks>
    public class ConsumingProject
    {
        private const string XRepoReferenceLabel = "XRepoReference";

        private static readonly Regex XRepoBlockPattern = new(
            @"(\r?\n)[ \t]*<ItemGroup\s+Label\s*=\s*""XRepoReference""[^>]*>[\s\S]*?</ItemGroup>[ \t]*",
            RegexOptions.Compiled);

        private static readonly Regex IndentPattern = new(
            @"^([ \t]+)<(?:ItemGroup|PropertyGroup)",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public static ConsumingProject Load(string filePath)
        {
            using var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true);
            var text = reader.ReadToEnd();
            var encoding = reader.CurrentEncoding;
            var doc = XDocument.Parse(text);
            return new ConsumingProject(doc, text, encoding, filePath);
        }

        private readonly XDocument _doc;
        private readonly string _originalText;
        private readonly Encoding _encoding;
        public string FilePath { get; }

        public ConsumingProject(XDocument doc, string filePath)
        {
            _doc = doc;
            _originalText = doc.ToString();
            _encoding = new UTF8Encoding(false);
            FilePath = filePath;
        }

        private ConsumingProject(XDocument doc, string originalText, Encoding encoding, string filePath)
        {
            _doc = doc;
            _originalText = originalText;
            _encoding = encoding;
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
            var text = RemoveXRepoBlock(_originalText);

            var xrepoItemGroups = GetXRepoItemGroups();
            if (xrepoItemGroups.Length > 0)
            {
                var indent = DetectIndent(text);
                var block = GenerateXRepoBlock(xrepoItemGroups, indent, DetectNewline(text));
                text = InsertBeforeClosingProject(text, block);
            }

            File.WriteAllText(FilePath, text, _encoding);
        }

        public bool ReferencesPackage(string packageId)
        {
            return _doc.Root!.Descendants(_doc.Root.Name.Namespace + "PackageReference")
                .Any(e => packageId.Equals((string?)e.Attribute("Include"), StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveXRepoProjectReferences()
        {
            var xrepoItemGroups = GetXRepoItemGroups();
            if (xrepoItemGroups.Length == 0)
                return false;

            foreach (var el in xrepoItemGroups)
                el.Remove();
            return true;
        }

        public bool RemoveXRepoProjectReference(string projectPath)
        {
            var ns = _doc.Root!.Name.Namespace;
            bool removed = false;

            foreach (var itemGroup in GetXRepoItemGroups())
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
            return GetXRepoItemGroups().Length > 0;
        }

        private XElement[] GetXRepoItemGroups()
        {
            var ns = _doc.Root!.Name.Namespace;
            return _doc.Root.Elements(ns + "ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == XRepoReferenceLabel)
                .ToArray();
        }

        private static string RemoveXRepoBlock(string text)
        {
            return XRepoBlockPattern.Replace(text, "");
        }

        private static string DetectIndent(string text)
        {
            var match = IndentPattern.Match(text);
            return match.Success ? match.Groups[1].Value : "  ";
        }

        private static string DetectNewline(string text)
        {
            return text.Contains("\r\n") ? "\r\n" : "\n";
        }

        private static string GenerateXRepoBlock(XElement[] xrepoItemGroups, string indent, string newline)
        {
            var sb = new StringBuilder();
            sb.Append(indent);
            sb.Append($@"<ItemGroup Label=""{XRepoReferenceLabel}"">");
            sb.Append(newline);

            foreach (var itemGroup in xrepoItemGroups)
            {
                foreach (var projectRef in itemGroup.Elements().Where(e => e.Name.LocalName == "ProjectReference"))
                {
                    var includePath = (string?)projectRef.Attribute("Include");
                    if (includePath != null)
                    {
                        sb.Append(indent);
                        sb.Append(indent);
                        sb.Append($@"<ProjectReference Include=""{includePath}"" />");
                        sb.Append(newline);
                    }
                }
            }

            sb.Append(indent);
            sb.Append("</ItemGroup>");
            sb.Append(newline);

            return sb.ToString();
        }

        private static string InsertBeforeClosingProject(string text, string block)
        {
            var closingTagIndex = text.LastIndexOf("</Project>", StringComparison.OrdinalIgnoreCase);
            if (closingTagIndex < 0)
                throw new InvalidOperationException("Could not find </Project> closing tag in csproj file.");

            return text.Insert(closingTagIndex, block);
        }
    }
}
