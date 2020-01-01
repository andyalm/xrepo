using System.Threading.Tasks;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_SETTINGNAME_config_setting_is_SETTINGVALUE : Step<XRepoEnvironmentContext>
    {
        private readonly string _settingName;
        private readonly string _settingValue;

        public the_SETTINGNAME_config_setting_is_SETTINGVALUE(string settingName, string settingValue)
        {
            _settingName = settingName;
            _settingValue = settingValue;
        }

        public override Task ExecuteAsync()
        {
            Context.Environment.ConfigRegistry.UpdateSetting(_settingName, _settingValue);
            Context.Environment.ConfigRegistry.Save();
            
            return Task.CompletedTask;
        }

    }
}