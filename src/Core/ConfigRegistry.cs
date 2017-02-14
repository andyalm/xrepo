using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XRepo.Core
{
    public class ConfigRegistry : JsonRegistry<ConfigSettings>
    {
        public static ConfigRegistry ForDirectory(string directoryPath)
        {
            return Load<ConfigRegistry>(directoryPath);
        }

        protected override string Filename
        {
            get { return "config.registry"; }
        }

        public void UpdateSetting(string name, string value)
        {
            var canonicalName = name.Replace("_", "").ToLowerInvariant();
            PropertyInfo configProperty;
            if(!Data.TryGetProperty(canonicalName, out configProperty))
                throw new XRepoException("The config setting '" + name + "' is unrecognized by xrepo");
            object typedValue;
            if(!value.TryChangeType(configProperty.PropertyType, out typedValue))
                throw new XRepoException("The config value '" + value + "' could not be converted to type '" + configProperty.PropertyType + "'");
            configProperty.SetValue(Data, typedValue); 
        }

        public ConfigSettings Settings
        {
            get { return Data; }
        }

        private List<ConfigSetting> _settingDescriptors; 
        public IEnumerable<ConfigSetting> SettingDescriptors
        {
            get
            {
                if(_settingDescriptors == null)
                {
                    _settingDescriptors = (from prop in Settings.GetType().GetTypeInfo().DeclaredProperties
                                           select new ConfigSetting(prop, Settings)).ToList();
                }

                return _settingDescriptors;
            }
        }
    }

    public class ConfigSetting
    {
        private readonly PropertyInfo _settingProperty;
        private readonly object _settingsObject;

        public ConfigSetting(PropertyInfo settingProperty, object settingsObject)
        {
            _settingProperty = settingProperty;
            _settingsObject = settingsObject;
            Name = ConvertNameToCommandLineFriendlyName();
        }

        public string Name { get; private set; }

        public string Value
        {
            get { return _settingProperty.GetValue(_settingsObject).ToString().ToLowerInvariant(); }
        }

        public string ConvertNameToCommandLineFriendlyName()
        {
            return _settingProperty.Name.ToSnakeCase();
        }
    }

    public class ConfigSettings
    {
        public bool CopyPins { get; set; }
        public bool PinWarnings { get; set; }
        public bool AutoBuildPins { get; set; }

        public ConfigSettings()
        {
            CopyPins = true;
            PinWarnings = true;
            AutoBuildPins = true;
        }
    }

    internal static class ConfigExtensions
    {
        public static bool TryGetProperty(this object obj, string propertyName, out PropertyInfo propertyInfo)
        {
            propertyInfo = obj.GetType().GetTypeInfo().DeclaredProperties.Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            
            return propertyInfo != null;
        }

        public static bool TryChangeType(this object obj, Type targetType, out object typedValue)
        {
            try
            {
                typedValue = Convert.ChangeType(obj, targetType);
                return true;
            }
            catch (InvalidCastException)
            {
                typedValue = null;
                return false;
            }
        }

        public static object GetValue(this PropertyInfo propertyInfo, object target)
        {
            return propertyInfo.GetValue(target, null);
        }

        public static void SetValue(this PropertyInfo propertyInfo, object target, object value)
        {
            propertyInfo.SetValue(target, value, null);
        }
    }
}