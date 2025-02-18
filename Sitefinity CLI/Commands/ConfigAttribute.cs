using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                    var value = configuration.GetValue(property.PropertyType, propertyKeyInConfig);
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
