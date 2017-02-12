using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kekiri;
using Kekiri.TestRunner.NUnit;
using NUnit.Framework;
using XRepo.Scenarios.Steps;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios
{
    public class a_compiled_assembly_is_registered : Scenario<XRepoEnvironmentContext>
    {
        public a_compiled_assembly_is_registered()
        {
            Given<a_class_library_project>();
            When<the_project_is_compiled>();
            Then<the_resulting_assembly_is_registered_by_xrepo>();
        }
    }

    public class a_pinned_assembly_overrides_hint_paths_by_default : Scenario<XRepoEnvironmentContext>
    {
        public a_pinned_assembly_overrides_hint_paths_by_default()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("XRepo.Scenarios")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("XRepo.Scenarios");
        }
    }

    public class a_pinned_assembly_is_copied_to_hint_path_location_when_copypins_is_true : Scenario<XRepoEnvironmentContext>
    {
        public a_pinned_assembly_is_copied_to_hint_path_location_when_copypins_is_true()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("XRepo.Scenarios")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "true");
            When<the_project_is_compiled>();
            Then<the_registered_copy_of_ASSEMBLYNAME_is_copied_to_the_hint_paths_location>("XRepo.Scenarios")
                .And<a_backup_copy_of_the_original_ASSEMBLYNAME_is_kept>("XRepo.Scenarios");
        }
    }

    public class An_unpinned_assembly_does_not_override_the_hint_path : Scenario<XRepoEnvironmentContext>
    {
        public An_unpinned_assembly_does_not_override_the_hint_path()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_not_pinned>("XRepo.Scenarios");
            When<the_project_is_compiled>();
            Then<the_reference_to_ASSEMBLYNAME_is_resolved_via_standard_msbuild_rules>("XRepo.Scenarios");
        }
    }

    public class A_pinned_repo_overrides_hint_paths_for_all_registered_assemblies_within_the_repo :
        Scenario<XRepoEnvironmentContext>
    {
        public A_pinned_repo_overrides_hint_paths_for_all_registered_assemblies_within_the_repo()
        {
            Given<a_repo_REPONAME>("MyRepo")
                .And<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("XRepo.Scenarios")
                .And<the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_REPONAME>("XRepo.Scenarios", "MyRepo")
                .And<the_repo_REPONAME_is_pinned>("MyRepo")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copy_pins", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("XRepo.Scenarios");
        }
    }
}
