using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kekiri;
using NUnit.Framework;
using XRepo.Scenarios.Steps;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios
{
    [TestFixture]
    public class HelloWorldFixture
    {
        [Test]
        public void HelloWorld() { }
    }

    public class A_compiled_assembly_is_registered : FluentTest<XRepoEnvironmentContext>
    {
        public A_compiled_assembly_is_registered()
        {
            Given<a_class_library_project>();
            When<the_project_is_compiled>();
            Then<the_resulting_assembly_is_registered_by_xrepo>();
        }
    }

    public class a_pinned_assembly_overrides_hint_paths_by_default : FluentTest<XRepoEnvironmentContext>
    {
        public a_pinned_assembly_overrides_hint_paths_by_default()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework");
        }
    }

    public class a_pinned_assembly_is_copied_to_hint_path_location_when_copypins_is_true : FluentTest<XRepoEnvironmentContext>
    {
        public a_pinned_assembly_is_copied_to_hint_path_location_when_copypins_is_true()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "true");
            When<the_project_is_compiled>();
            Then<the_registered_copy_of_ASSEMBLYNAME_is_copied_to_the_hint_paths_location>("nunit.framework")
                .And<a_backup_copy_of_the_original_ASSEMBLYNAME_is_kept>("nunit.framework");
        }
    }

    public class An_unpinned_assembly_does_not_override_the_hint_path : FluentTest<XRepoEnvironmentContext>
    {
        public An_unpinned_assembly_does_not_override_the_hint_path()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_not_pinned>("nunit.framework");
            When<the_project_is_compiled>();
            Then<the_reference_to_ASSEMBLYNAME_is_resolved_via_standard_msbuild_rules>("nunit.framework");
        }
    }

    public class A_pinned_repo_overrides_hint_paths_for_all_registered_assemblies_within_the_repo :
        FluentTest<XRepoEnvironmentContext>
    {
        public A_pinned_repo_overrides_hint_paths_for_all_registered_assemblies_within_the_repo()
        {
            Given<a_repo_REPONAME>("MyRepo")
                .And<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_REPONAME>("nunit.framework", "MyRepo")
                .And<the_repo_REPONAME_is_pinned>("MyRepo")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copy_pins", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework");
        }
    }

    public class A_pinned_assembly_is_resolved_at_a_specific_version_by_default : FluentTest<XRepoEnvironmentContext>
    {
        public A_pinned_assembly_is_resolved_at_a_specific_version_by_default()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework")
                .And<the_build_should_not_give_warning_WARNINGTEXT>("Could not resolve this reference. Could not locate the assembly \"nunit.framework")
                .And<the_build_should_succeed>();
        }
    }

    public class A_pinned_assembly_is_resolved_with_specific_version_when_setting_is_false :
        FluentTest<XRepoEnvironmentContext>
    {
        public A_pinned_assembly_is_resolved_with_specific_version_when_setting_is_false()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework, Version=2.6.4, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("specificversion", "false");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework")
                .And<the_build_should_not_give_warning_WARNINGTEXT>("Could not resolve this reference. Could not locate the assembly \"nunit.framework")
                .And<the_build_should_succeed>();
        }
    }

    public class A_pinned_assembly_is_resolved_at_a_specific_version_when_setting_is_true :
        FluentTest<XRepoEnvironmentContext>
    {
        public A_pinned_assembly_is_resolved_at_a_specific_version_when_setting_is_true()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("specificversion", "true");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework")
                .And<the_build_should_not_give_warning_WARNINGTEXT>("Could not resolve this reference. Could not locate the assembly \"nunit.framework")
                .And<the_build_should_succeed>();
        }
    }

    public class A_pinned_assembly_is_resolved_with_specific_version_when_setting_is_true :
        FluentTest<XRepoEnvironmentContext>
    {
        public A_pinned_assembly_is_resolved_with_specific_version_when_setting_is_true()
        {
            Given<a_class_library_project>()
                .And<the_project_has_a_reference_to_assembly_ASSEMBLYNAME>("nunit.framework, Version=2.6.4, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL")
                .And<the_assembly_ASSEMBLYNAME_is_registered>("nunit.framework")
                .And<the_assembly_ASSEMBLYNAME_is_pinned>("nunit.framework")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("copypins", "false")
                .And<the_SETTINGNAME_config_setting_is_SETTINGVALUE>("specificversion", "true");
            When<the_project_is_compiled>();
            Then<the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME>("nunit.framework")
                .And<the_build_should_give_warning_WARNINGTEXT>("Could not resolve this reference. Could not locate the assembly \"nunit.framework")
                .And<the_build_should_succeed>();
        }
    }
}
