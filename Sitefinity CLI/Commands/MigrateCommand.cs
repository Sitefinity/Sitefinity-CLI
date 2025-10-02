using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using EnvDTE;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using Sitefinity_CLI.Exceptions;
using Newtonsoft.Json.Linq;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Enums;
using AllowedValuesAttribute = McMaster.Extensions.CommandLineUtils.AllowedValuesAttribute;
using Progress.Sitefinity.MigrationTool.Core;
using Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using static Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.WidgetMigrationDefaults;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.MigrateCommandName, Description = "Migrate a Sitefinity page/template from Web Forms and MVC to ASP.NET Core or Next.js. See https://www.progress.com/documentation/sitefinity-cms/migrate-your-project-with-cli")]
    internal class MigrateCommand
    {
        [Argument(0, Description = Constants.PresentationTypeDescription)]
        [Required(ErrorMessage = "You must specify a resource type - page/template/responses.")]
        [AllowedValues("page", "template", "responses", IgnoreCase = true)]
        public string Type { get; set; }

        [Argument(1, Description = Constants.ResourceId)]
        [Required(ErrorMessage = "You must pass the id of the page/template. You can retrieve it from the analyzer page.")]
        public string Id { get; set; }

        [Config]
        [Option(Constants.MigrationRecreateTemplate, Description = Constants.RecreateOption)]
        public bool Recreate { get; set; }

        [Config]
        [Option(Constants.MigrationRecursiveTemplate, Description = Constants.RecursiveOption)]
        public bool Recursive { get; set; }

        [Config]
        [Option(Constants.MigrationReplaceTemplate, Description = Constants.ReplaceOption)]
        public bool Replace { get; set; }

        [Config]
        [Option(Constants.MigrationFrameworkTemplate, Description = Constants.MigrationFrameworkOption)]
        [AllowedValues("NetCore", "NextJS", IgnoreCase = true)]
        public string Framework { get; set; }

        /*[Config]
        [Option(Constants.DumpSourceLayoutTemplate, Description = Constants.DumpOption)]
        public bool DumpSourceLayout { get; set; }

        [Option(Constants.MigrationActionTemplate, Description = Constants.MigrateAction)]
        [AllowedValues("draft", "publish", IgnoreCase = true)]
        [Config]
        public string Action { get; set; } = "draft";*/

        [Config]
        [Option(Constants.MigrationCmsUrlTemplate, Description = Constants.CmsUrl)]
        public string CmsUrl { get; set; }

        [Option(Constants.MigrationTokenTemplate, Description = Constants.AuthToken)]
        [Config]
        public string Token { get; set; }

        [Option(Constants.MigrationSiteTemplate, Description = Constants.SiteAction)]
        [Config]
        public string SiteId { get; set; }

        [Config]
        public Dictionary<string, WidgetMigrationArgs> Widgets { get; set; }

        [Config]
        public Dictionary<string, string> PlaceholderMap { get; set; }

        [Config]
        public Dictionary<string, string> FormFieldNameMap { get; set; }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (string.IsNullOrEmpty(this.CmsUrl))
            {
                throw new ValidationException("You must specify a CMS URL.");
            }

            if (string.IsNullOrEmpty(this.Token))
            {
                throw new ValidationException("You must pass an authentication token. See https://www.progress.com/documentation/sitefinity-cms/generate-access-key on how to generate one.");
            }

            var allWidgets = new Dictionary<string, WidgetMigrationArgs>(WidgetMigrationDefaults.MigrationMap);
            if (this.Widgets != null)
            {
                foreach (var widget in this.Widgets)
                {
                    if (allWidgets.TryGetValue(widget.Key, out var existing))
                    {
                        allWidgets[widget.Key] = widget.Value;
                    }
                    else
                    {
                        allWidgets.Add(widget.Key, widget.Value);
                    }
                }
            }

            if (this.Type == "page")
            {
                await Migrator.MigratePages(new PageMigrationArgs([this.Id], this.CmsUrl, this.Token, allWidgets, WidgetMigrationDefaults.CustomMigrations)
                {
                    Recreate = this.Recreate,
                    Replace = this.Replace,
                    SiteId = this.SiteId,
                    Recursive = this.Recursive,
                    DefaultWidgetMigration = new PlaceholderWidget()
                });
            }
            else if (this.Type == "template")
            {
                await Migrator.MigrateTemplates(new TemplateMigrationArgs([this.Id], this.CmsUrl, this.Token, allWidgets, WidgetMigrationDefaults.CustomMigrations)
                {
                    Recreate = this.Recreate,
                    SiteId = this.SiteId,
                    Recursive = this.Recursive,
                    PlaceholderMap = this.PlaceholderMap,
                    Framework = string.Equals("NextJS", this.Framework, StringComparison.OrdinalIgnoreCase) ? RendererFramework.NextJS : RendererFramework.NetCore,
                    DefaultWidgetMigration = new PlaceholderWidget()
                });
            }
            else if (this.Type == "responses")
            {
                await Migrator.MigrateFormResponses(new FormResponsesMigrationArgs([this.Id], this.CmsUrl, this.Token, this.FormFieldNameMap));
            }

            return (int)ExitCode.OK;
        }
    }
}
