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
    public class a_compiled_assembly_is_registered : FluentTest<XRepoEnvironmentContext>
    {
        public a_compiled_assembly_is_registered()
        {
            Given<a_class_library_project>();
            When<the_project_is_compiled>();
            Then<the_resulting_assembly_is_registered_by_xrepo>();
        }
    }
}
