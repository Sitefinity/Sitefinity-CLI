# Sitefinity CLI

## Overview

You use Sitefinity CLI to perform maintainance tasks on your Sitefinity project, such as creating a new project, adding resources to an existing project, updating the project to a newer version, or migrating the front-end rendering framework.

## Prerequisites

  To use or build the CLI, you need to install the corresponding version of the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

## Installation

* Use prebuild version

  You can download a prebuild version for some operating systems from the [release assets](https://github.com/Sitefinity/Sitefinity-CLI/releases). Extract the archive to a folder of your choice and add this folder to the ```PATH``` system variable.

## Build the app yourself

  To build the application for your OS, enter the following command from the project root folder:

  ```dotnet publish -c release -r [rid]```

  **NOTE**: Replace [rid] with the identifier for your OS. For more information, see the [.NET Core RID Catalogue](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

  **EXAMPLE**: To build the app for Windows x64, enter the following command:

  ```dotnet publish -c release -r win-x64```

  and add the following path to the PATH System variable:

  ```(project_root_path)\bin\Release\(SDKver)\(rid)\publish```

## How to use

* Open command prompt and navigate to the root of your Sitefinity project (`SitefinityWebApp` folder).
* Run ```sf``` command.

  A help will appear describing the available commands and options.

## CLI commands

You can use the add command with the following subcommands:

* To create a new **Resource package**, execute the following command:

  ```sf add package "Test resource package"```

* To create a new **Page template**, execute the following command:

  ```sf add pagetemplate "Test page"```

* To create a new **Grid widget**, execute the following command:

  ```sf add gridwidget "Test grid"```

* To create a new **Widget**, execute the following command:

  ```sf add widget "CustomWidget"```

* To create a new **Integration Tests Project**, execute the following command:

  ```sf add tests "Sitefinity.Tests.Integration"```

* To create a new **Custom Module**, execute the following command:

  ```sf add module "Custom Module"```

* To **Create** a new Sitefinity project, execute the following command:

  ```sf create TestProject```

  You can also specify a directory for the project to be installed in after the name (By default the current directory is used).

  ```sf create TestProject "D:\TestProjectFolder"```

  Add ```--headless``` to the command to install the headless version of Sitefinity.

  Add ```--coreModules```  to the command to install the core modules only version of Sitefinity.

  Run the help option to see all available install options and configurations.

* To **Upgrade** your project, execute the following command:

  ```sf upgrade "(path-to-your-project)\SitefinityWebApp.sln" "13.0.7300"```

  For more information, see [Upgrade using Sitefinity CLI](https://www.progress.com/documentation/sitefinity-cms/upgrade-using-sitefinity-cli).

**NOTE**: For more information about the arguments and options for each command, run the help option:

```sf add [command name] -?```

## CLI migration commands

You use the migration commands to migrate the front-end resources written in Web Forms or MVC to a decoupled renderer.

* To migrate a page, execute the following command:

  ```sf migrate page "PageId"```

* To migrate a page template, execute the following command:

  ```sf migrate template "TemplateId"```

**NOTE** For a list of all available options, execute the following command or refer to documentation [here](#migration-commands)):

  ``` sf migrate help ```

## Sitefinity CMS version

Every command has an option ```--version```. You use it to tell the CLI which template version is used in the generation process. You can find the templates in the ```Templates``` folder, which contains separate folders for each Sitefinity CMS version, starting from 10.2.

When running a command, the CLI tries to automatically detect your Sitefinity CMS project version and use the corresponding template. If it cannot detect the version or your Sitefinity CMS version is higher than latest templates version, CLI will use the latest available one.

You can use the ```--version``` option to explicitly set the templates version that CLI should use.

**EXAMPLE**: Following is an example command of using the ```–-version``` option:
```
sf add package "New resource package" --version "11.0"
```
In this case, the CLI will look for a folder named ```11.0``` inside folder ```Templates```. Folder 11.0 must have a ```ResourcePackage``` folder containing templates for a resource package.

## Template generation

When you run a command, the CLI prompts you to enter the name of the template to be used for the generation. You can also set the name using option ```--template```.

**EXAMPLE**: Following is an example command of using the ```-–template``` option:
```
sf add pagetemplate "New page" --template "CustomPageTemplate"
```
In this case, the CLI will look for a file ```CustomPageTemplate.Template``` in the folder ```Templates(version)\Page```.

### Custom templates

Templates use Handlebars syntax. For more information, see [Handlebars.Net](https://github.com/rexm/Handlebars.Net).

You can easily create custom templates. To do this, create a file with extension ```.Template``` and place it in the corresponding folder. If the template contains some properties, you should also create a ```(templateName).config.json``` file. It must contain all the properties used in the template. The CLI will read the ```.config``` file and prompt you to enter the properties when the template is selected.

**EXAMPLE**: Following is a sample template file:
```
{{> sign}}

{{message}}
{{time}}
{{age}}
```

**EXAMPLE**: Following is a sample config file:
```json
[
  "message",
  "time",
  "age"
]
```
**NOTE**: The partial ```{{> sign}}``` is automatically populated by the CLI.

## Migration Commands
The migration commands support migration of pages and page templates that are built using Web Forms or MVC to the decoupled architecture.

**NOTE**: The migration tool does not migrate your code. It migrates content and structure only.

### General flow of migration

**RECOMMENDATION**: We recommend to analyze and evaluate the state of your Sitefinity project and to estimate the resources needed for migration before starting the migration itself.
For more information, see [Technology migration](https://www.progress.com/documentation/sitefinity-cms/technology-migration).

To migrate your Sitefinity CMS project, perform the following procedure:

* Start with the migration of templates that a subset of pages is based on OR migrate all of the page templates at once.
* Make adjustments to the migrated structure as needed:
  * Set a file system template.
  * Migrate the widgets used on the template
    * Take a business decision if you can stop using some of the existing widgets. For example, drop widgets if the busness need is no longer there.
    * Manually configure the widgets that are migrated.
  * Make adjustment to the look and feel of the page.
* Once you complete the migrationof the chosen page templates, move on to the pages structure.
  * Sitefinity CLI duplicates the Pages by default and excludes the copies from the navigation.
  * Once you fully migrate the page, specify the --replace option to the CLI `migrate page` command.

**NOTE**: You can specify the `--replace` option only when the page has a duplicate.

**NOTE**: The CLI uses the page's and the template's `Title` property to identify the page or the page template that it created. The format has a suffix of `(migrated to Decoupled)`. This process avoids conflicts with existing pages and page templates.

**NOTE**: The CLI uses only data from the published pages and page templates. `Draft` and `Temp` changes are not migrated.

**NOTE**: Migration of forms is automatic if there is a form widget on the page.

### Migrating hierarchies
* When you select a page template, the CLI migrates first the parent page templates. Migration cannot happen otherwise.
* If parent page templates are migrated automatically, they will be also published automatically.

**NOTE**: When migrating pages, the CLI will not migrate the parent page templates. All dependent page templates must be migrated before migrating pages.

### Safe box & Testing
All pages and page templates are duplicated by default with a suffix in the their `Title` `(migrated to Decoupled)`. This provides a level of isolation for existing pages and page templates, so that the migration can happen seamlessly and without downtime. This is a great way to test the changes before they go live.

**NOTE**: Pages support the option `--replace`. See [Migration options](#migration-options). This replaces the page contents on the **ACTUAL** page and saves them as a draft. Thanks to this option, existing links from content blocks, html fields, and related data are kept and you do not need to update these references. When using the `--replace` option, the page is automatically saved as Draft, regardless of the value of the --action option.

**NOTE**: Duplicated Pages are hidden from navigation by default.

### Required parameters
* CMS URL ('--cmsUrl')<br>
The URL of the deployed Sitefinity CMS.<br>
For example, `https://www.example.com/en`
* Token ('--token')<br>
The authentication token to use.<br>
To learn how to generate a token, see [Generate access key](https://www.progress.com/documentation/sitefinity-cms/generate-access-key). You must use a token of an account with full `Administrator` privileges.

### Optional parameters
* `--recreate`<br>
Recreates the selected page or template **and** its parent templates.<br>
Useful when testing and experimenting with custom configurations/custom widget migrations
* `--recursive`<br>
Recursively migrates all the child pages or templates of the selected page/template. When migrating templates, the tool does not recursively migrate pages.
* '--replace'<br>
Replaces the content of the page. Valid only for pages.
* '--action'<br>
The action to execute at the end of the migration. Allowed values are:
  - `draft` - Save the migrated resource as Draft. 
  - `publish` - Publish the migrated resource.
* `--siteid`<br>
The site id. You use the --siteid parameter to specify the site id when you work with a non-default site.

**NOTE**: All parameters can be manually specified in the appsettings.json file. **You need to manually create this file next to the sf.exe binary.**

**EXAMPLE**: 
``` json

{
  "Commands": {
    "Migrate": {
      "CmsUrl": "http://localhost",
      "Token": "TOKEN",
      "Recreate": false,
      "Recursive": false,
      "Replace": false,
      "Action": "publish",
      "SiteId": "00000000-0000-0000-0000-000000000000",
      "Widgets": {
          "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock": {
              "Name": "SitefinityContentBlock",
              "Whitelist": ["Html", "ProviderName", "SharedContentID"],
              "Rename": {
                "Html": "Content"
              },
              "CleanNullProperties": true
          }
      }

    }
  }
}

```

You can mix both appsettings.json parameters and direct command-line parameters, with the latter having precedence.

### Widget migration
You have two options for migration of widgets:
* Through configuration
* Through custom widget migrations

### Migration through configuration
This can be specified in the appsettings.json file. For example:
``` json

{
  "Commands": {
    "Migrate": {
      ...
      "Widgets": {
          "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock": {
              "Name": "SitefinityContentBlock", // the name of the new widget in NetCore/NextJs renderers
              "Whitelist": ["Html", "ProviderName", "SharedContentID"], // the whitelist of properties to keep during the migration
              "Rename": { // the properties to be renamed
                "Html": "Content"
              },
              "CleanNullProperties": true
          }
      }
    }
  }
}

```

### Custom widget migrations

Custom widget migrations can be used when the configuration is not sufficient and a more complex logic is required. All built-in widget migrations are located under the `Migrations` folder for reference.

Widget migrations are invoked for each **occurrence** of the widget found on the page or page template. For each invocation, a `WidgetMigrationContext` is passed. It contains:

* `SourceClient` -> The `IRestClient` for interfacing with the CMS
* `Source` -> The original widget, from which you are migrating.
* `ParentId` -> The new parent id for the migrated widget.
* `Language` -> The current language.
* `SegmentId` -> The page segment when working with personalized pages.
* `WidgetSegmentId` -> The widget segment when working with personalized widgets.
* `LogWarning` -> specifies a warning message to be shown in the Terminal during during the migration of the widget.
* `LogInfo` -> specifies an informational message to be shown in the Terminal during during the migration of the widget.
* `SiteId` -> The site id. You use the SiteId parameter to specify the site id when you work with a non-default site.

A return value of type `MigratedWidget` is required as the output of the migration. It contains the new widget name and properties.

### Helpful functions

From base class `MigrationBase`

* `ProcessProperties` -> Copes and renames properties.
* `GetMasterIds` -> Gets the master ids of the live content items (usually referenced in Web Forms widgets).
* `GetSingleItemMixedContentValue`, `GetMixedContentValue` -> Helper for generating properties of type `MixedContentContext`.

### Limitations
* The CLI migration command does not migrate the code in any form. It can migrate only the content and structure of your project.

## Known issues
#### Visual Studio 2015 integration
Visual Studio 2015 won't refresh the solution correctly after the Sitefinity VSIX/CLI correctly update the .csproj and .sln files.
To work around the issue, reopen the solution after the CLI modifies it.
