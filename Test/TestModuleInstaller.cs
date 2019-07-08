using System;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Services;

namespace Test
{
    public class TestModuleInstaller
    {
        public static void PreApplicationStart()
        {
            Bootstrapper.Initialized += Bootstrapper_Initializing;
        }

        public static void Bootstrapper_Initializing(object sender, EventArgs e)
        {
            var moduleName = TestModule.moduleName;

            if (!Config.Get<SystemConfig>().ApplicationModules.ContainsKey(moduleName))
            {
                var configManager = ConfigManager.GetManager();
                var modulesConfig = configManager.GetSection<SystemConfig>().ApplicationModules;

                var moduleSettings = new AppModuleSettings(modulesConfig)
                {
                    Name = "Test",
                    Type = typeof(TestModule).AssemblyQualifiedName,
                    Title = moduleName,
                    Description = "Custom module for testing",
                    StartupType = StartupType.OnApplicationStart
                };

                modulesConfig.Add(moduleName, moduleSettings);
                configManager.SaveSection(modulesConfig.Section);
            }
        }
    }
}