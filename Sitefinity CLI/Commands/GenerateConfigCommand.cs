using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.GenerateConfigCommandName, Description = "Generates a configuration file describing the available commands. The file is to be used by the Sitefinity DevKit.")]
    internal class GenerateConfigCommand
    {
        private string[] optionsToSkip = { "ProjectRootPath", "Version" };
        private string[] mainCommandsToSkip = { Constants.GenerateConfigCommandName };

        protected int OnExecute(CommandLineApplication app)
        {
            var program = Assembly.GetExecutingAssembly().GetType("Sitefinity_CLI.Program");
            var subCommandMainCommandAttributes = program.GetCustomAttributes(typeof(SubcommandAttribute));
            var config = new List<CommandModel>();

            foreach (SubcommandAttribute subCommandMainCommandAttribute in subCommandMainCommandAttributes)
            {
                if (mainCommandsToSkip.Contains(subCommandMainCommandAttribute.Name))
                {
                    continue;
                }

                var mainCommandType = subCommandMainCommandAttribute.CommandType;
                var subCommandAttributes = mainCommandType.GetCustomAttributes(typeof(SubcommandAttribute));

                foreach (SubcommandAttribute subCommandAttribute in subCommandAttributes)
                {
                    var commandType = subCommandAttribute.CommandType;

                    var commandAttribute = commandType.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;
                    var commandModel = new CommandModel();

                    // title and name
                    commandModel.Name = string.Format("{0} {1}", subCommandMainCommandAttribute.Name, commandAttribute.Name);
                    commandModel.Title = commandAttribute.FullName;

                    // arguments
                    var argumentProps = commandType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ArgumentAttribute)));
                    var arguments = new List<string>();
                    foreach (var argument in argumentProps)
                    {
                        arguments.Add(argument.Name);
                    }
                    commandModel.Args = arguments;

                    // options
                    var optionProps = commandType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(OptionAttribute)));
                    var options = new List<OptionModel>();
                    foreach (var option in optionProps)
                    {
                        if (optionsToSkip.Contains(option.Name))
                        {
                            continue;
                        }

                        var optionAttr = option.GetCustomAttribute(typeof(OptionAttribute)) as OptionAttribute;
                        var defaultValueAttr = option.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                        var optionModel = new OptionModel();
                        optionModel.Name = optionAttr.Template.Split('|').First();
                        optionModel.Title = option.Name;
                        optionModel.DefaultValue = defaultValueAttr.Value.ToString();
                        options.Add(optionModel);
                    }
                    commandModel.Options = options;

                    config.Add(commandModel);
                }
            }
            
            var content = JsonConvert.SerializeObject(config);
            var configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
            File.WriteAllText(configFilePath, content);
            Utils.WriteLine(string.Format(Constants.ConfigFileCreatedMessage, configFilePath), ConsoleColor.Green);
            return 0;
        }
    }
}
