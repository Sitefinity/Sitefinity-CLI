using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Progress.Sitefinity.MigrationTool.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class ConfigAttribute : Attribute, IMemberConvention
    {
        public void Apply(ConventionContext context, MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                context.Application.OnParsingComplete((_) =>
                {
                    var commandName = property.DeclaringType.GetCustomAttribute<CommandAttribute>().Name;
                    var propertyName = property.Name;

                    var configuration = context.Application.GetRequiredService<IConfiguration>();
                    var propertyKeyInConfig = $"Commands:{commandName}:{propertyName}";
                    object value = null;
                    if (property.PropertyType == typeof(Dictionary<string, string>))
                    {
                        value = configuration.GetSection(propertyKeyInConfig).GetChildren().ToDictionary(x => x.Key, x => x.Value);
                    }
                    else if (property.PropertyType == typeof(Dictionary<string, WidgetMigrationArgs>))
                    {
                        value = configuration.GetSection(propertyKeyInConfig).GetChildren().ToDictionary(x => x.Key, (x) =>
                        {
                            var widgetMigrationArgs = new WidgetMigrationArgs(x.Key);
                            x.Bind(widgetMigrationArgs);
                            return widgetMigrationArgs;
                        });
                    }
                    else
                    {
                        value = configuration.GetValue(property.PropertyType, propertyKeyInConfig);
                    }
                    
                    if (value != null)
                    {
                        var currentValue = property.GetValue(context.ModelAccessor.GetModel());
                        if (currentValue == null)
                        {
                            property.SetValue(context.ModelAccessor.GetModel(), value);
                        }
                    }
                });
            }
        }
    }
}
