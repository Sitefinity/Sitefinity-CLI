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
    [Command(Constants.MigrateCommandName, Description = "Migrate a Sitefinity page/template from Web Forms/MVC to NetCore/NextJS")]
    internal class MigrateCommand
    {
        [Argument(0, Description = Constants.PresentationTypeDescription)]
        [Required(ErrorMessage = "You must specify a resource type - page/template.")]
        [AllowedValues("page", "template", IgnoreCase = true)]
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
        [Option(Constants.DumpSourceLayoutTemplate, Description = Constants.DumpOption)]
        public bool DumpSourceLayout { get; set; }

        [Option(Constants.MigrationActionTemplate, Description = Constants.MigrateAction)]
        [AllowedValues("draft", "publish", IgnoreCase = true)]
        [Config]
        public string Action { get; set; } = "draft";

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

            var log = new LogArgs()
            {
                DumpOriginalLayoutStateToFile = this.DumpSourceLayout,
            };

            var defaultMigration = new PlaceholderWidget();
            var action = Enum.Parse<SaveAction>(this.Action, true);

            if (this.Type == "page")
            {
                await Migrator.MigratePages(new PageMigrationArgs([this.Id], this.CmsUrl, this.Token, WidgetMigrationDefaults.MigrationMap, WidgetMigrationDefaults.CustomMigrations)
                {
                    Recreate = this.Recreate,
                    Action = action,
                    Log = log,
                    ReplacePageContent = this.Replace,
                    SiteId = this.SiteId,
                    Recursive = this.Recursive
                });
            }
            else if (this.Type == "template")
            {
                await Migrator.MigrateTemplates(new TemplateMigrationArgs([this.Id], this.CmsUrl, this.Token, WidgetMigrationDefaults.MigrationMap, WidgetMigrationDefaults.CustomMigrations)
                {
                    Recreate = this.Recreate,
                    Action = action,
                    Log = log,
                    SiteId = this.SiteId,
                    Recursive = this.Recursive,
                    PlaceholderMap = new Dictionary<string, string>()
                    {
                        { "Contentplaceholder1", "Body" }
                    }
                });
            }

            return (int)ExitCode.OK;
        }
    }
}
