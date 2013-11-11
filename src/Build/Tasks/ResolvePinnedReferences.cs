using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class ResolvePinnedReferences : XRepoTask
    {
        private readonly List<ITaskItem> _projectReferences = new List<ITaskItem>();
        private readonly List<ITaskItem> _assemblyReferenceRemovals = new List<ITaskItem>();
        private readonly List<ITaskItem> _assemblyReferenceAdditions = new List<ITaskItem>();
        
        [Required]
        public ITaskItem[] AssemblyReferences { get; set; }

        public bool SkipUnchangedFiles { get; set; }

        [Output]
        public ITaskItem[] ProjectReferences
        {
            get { return _projectReferences.ToArray(); }
        }

        [Output]
        public ITaskItem[] AssemblyReferenceRemovals
        {
            get { return _assemblyReferenceRemovals.ToArray(); }
        }

        [Output]
        public ITaskItem[] AssemblyReferenceAdditions
        {
            get { return _assemblyReferenceAdditions.ToArray(); }
        }

        public ResolvePinnedReferences()
        {
            SkipUnchangedFiles = true;
        }

        public override void ExecuteOrThrow()
        {
            foreach (var assemblyReference in AssemblyReferences)
            {
                var assemblyName = assemblyReference.AssemblyShortName();
                var pinnedProject = Environment.FindPinForAssembly(assemblyName);
                if (pinnedProject == null)
                    continue;
                var configSettings = Environment.ConfigRegistry.Settings;
                if (configSettings.PinWarnings)
                    Log.LogWarning("The assembly '" + assemblyName + "' is pinned");
                if (!string.IsNullOrEmpty(pinnedProject.Project.ProjectPath))
                {
                    ReplaceAssemblyReferenceWithProjectReference(assemblyReference, pinnedProject.Project.ProjectPath, assemblyName);
                }
                else if (configSettings.CopyPins && assemblyReference.ContainsMetadata("HintPath"))
                {
                    CopyPinnedAssembly(assemblyName, pinnedProject, assemblyReference.GetMetadata("HintPath"));
                }
                else
                {
                    OverrideHintPath(assemblyName, pinnedProject.Project.AssemblyPath, assemblyReference);
                }
            }
        }

        private void ReplaceAssemblyReferenceWithProjectReference(ITaskItem assemblyReference, string projectPath, string assemblyName)
        {
            Log.LogMessage("Replacing assembly reference to '" + assemblyName + "' with project reference to '" + projectPath + "'...");
            
            _assemblyReferenceRemovals.Add(assemblyReference);
            var projectReference = new TaskItem(projectPath);
            projectReference.SetMetadata("Project", GetProjectGuid(projectPath, assemblyName));
            projectReference.SetMetadata("Name", projectReference.Filename());
            _projectReferences.Add(projectReference);
        }

        private string GetProjectGuid(string projectPath, string assemblyName)
        {
            var project = XDocument.Load(projectPath);
            XNamespace msb = "http://schemas.microsoft.com/developer/msbuild/2003";
            var projectGuidElement = project.Descendants(msb + "ProjectGuid").FirstOrDefault();
            if(projectGuidElement == null)
                throw new ApplicationException("Unable to find the ProjectGuid for project '" + projectPath + "'. Is this project supposed to be associated with assembly '" + assemblyName + "'?");
            
            return projectGuidElement.Value;
        }

        private void CopyPinnedAssembly(string assemblyName, PinnedProject project, string hintPath)
        {
            var backupEntry = project.Pin.GetBackupForAssembly(assemblyName);
            var hintPathDir = Path.GetFullPath(Path.GetDirectoryName(hintPath));
            if (!backupEntry.ContainsOriginalDirectory(hintPathDir))
            {
                var newBackupEntry = backupEntry.AddEntry(Environment.Directory, hintPathDir);
                Environment.PinRegistry.Save();
                Log.LogMessage("Backing up original copy of assembly '" + assemblyName + "' in '" + hintPath + "' because it is about to be overridden by a pinned copy of that assembly...");
                this.ExecTask(() => new CopyAssembly
                {
                    Assemblies = hintPath.ToTaskItems(),
                    DestinationFolder = newBackupEntry.ToTaskItem(),
                    CopyDependencies = false,
                    SkipUnchangedFiles = SkipUnchangedFiles
                });
            }
            Log.LogMessage("Copying pinned assembly '" + assemblyName + "' to hint path location '" + hintPath + "'...");
            this.ExecTask(() => new CopyAssembly
            {
                Assemblies = project.Project.AssemblyPath.ToTaskItems(),
                DestinationFolder = hintPath.ToTaskItem().FullDirectoryPath().ToTaskItem(),
                CopyDependencies = false,
                SkipUnchangedFiles = SkipUnchangedFiles
            });
        }

        private void OverrideHintPath(string assemblyName, string pinnedAssemblyPath, ITaskItem assemblyReference)
        {
            Log.LogMessage("Overriding assembly reference '" + assemblyName + "' to use pinned path '" +
                           pinnedAssemblyPath + "'...");
            assemblyReference.SetMetadata("HintPath", pinnedAssemblyPath);
            assemblyReference.SetMetadata("ShortName", assemblyName);

            _assemblyReferenceAdditions.Add(assemblyReference);
            _assemblyReferenceRemovals.Add(new TaskItem(assemblyReference.ItemSpec));
        }
    }
}