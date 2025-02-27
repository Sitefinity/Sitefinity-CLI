# Sitefinity CLI

## Prerequisites

  To use or build the CLI, you need to install the corresponding version of the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## Installation

* Use prebuild version

  You can download a prebuild version for some operating systems from the [release assets](https://github.com/Sitefinity/Sitefinity-CLI/releases). Extract the archive to a folder of your choice and add this folder to the ```PATH``` system variable.

## Build the app yourself

  To build the application for your OS, enter the following command from the project root folder:

  ```dotnet publish -c release -r [rid]```

  **NOTE**: Replace [rid] with the identifier for your OS. For more information, see the [.NET Core RID Catalogue](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

  **EXAMPLE**: To build the app for Windows 10 x64, enter the following command:

  ```dotnet publish -c release -r win10-x64```

  and add the following path to the PATH System variable:

  ```(project_root_path)\bin\release\netcoreapp2.0\(rid)\publish```

## How to use

* Open command prompt and navigate to the root of your Sitefinity project (SitefinityWebApp folder).
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

  ```sf upgrade "D:\TestProject\SitefinityWebApp.sln" "13.0.7300"```

  For more information, see [Upgrade using Sitefinity CLI](https://www.progress.com/documentation/sitefinity-cms/upgrade-using-sitefinity-cli).

**NOTE**: For more information about the arguments and options for each command, run the help option:

```sf add [command name] -?```

## CLI migration commands

* To migrate a page, execute the following command:

  ```sf migrate page "PageId"```

* To migrate a page template, execute the following command:

  ```sf migrate template "TemplateId"```

**NOTE** For a list of all available options, execute the following command(or refer to docs [here](#migration-commands)):

  ``` sf migrate help ```

## Sitefinity CMS version

Every command has an option ```--version```. It is used to tell the CLI which template version should be used in the generation process. Templates can be found in the ```Templates``` folder, in separate folders for each Sitefinity CMS version, starting from 10.2.

When running a command the CLI will try to automatically detect your Sitefinity CMS project version and use the corresponding template. If it cannot detect the version or your Sitefinity CMS version is higher than latest templates version, CLI will use the latest available.

You can use the ```--version``` option to explicitly set the templates version that CLI should use.

**EXAMPLE**: Following is an example command of using the ```–-version``` option:
```
sf add package "New resource package" --version "11.0"
```
In this case, the CLI will look for a folder named ```11.0``` inside folder ```Templates```. Folder 11.0 must have ```ResourcePackage``` folder containing templates for a resource package.

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
The migration commands support migration of pages/templates that are build with the WebForms/MVC to the decoupled architecture.

### General flow of migration

* Start with the migration of a template/templates that a subset of pages is based on OR migrate all of the page templates at once.
* Make adjustments to the migrated structure:
  * Set a file system template
  * Manually configure the widgets that are migrated.
  * Make adjustment to the look and feel of the page.

* Once selected the page templates are migrated, move on to the pages structure
  * Pages are duplicated by default and excluded from the navigation.
  * Once the page is fully migrated, specify the --replace option

### Migrating hierarchies
* When a page/page template is selected, first the parent page templates are migrated. Migration cannot happen otherwise.
* If there are parent page templates automatically migrated, they will be automatically published

### Safe box & Testing
All pages and page templates are duplicated by default with a suffix in the Title(migrated to Decoupled). This provides a level of isolation for existing pages/page templates, so that the migration can happen seamless and without downtime. Additionally, this is a great way to test the changes before they go live.

**NOTE** Pages support the option (--replace. See [Migration options](#migration-options)). This replaces the page contents on the **ACTUAL** page and saves them as draft. Thanks to this option, existing links from content blocks/html fields/related data are kept and no effort is required to update those references. When using the Replace option, the page is automatically saved as Draft (regardless of the value of the --action option)

**Duplicated Pages are hidden from navigation by default.**

### Required parameters
* Cms Url ('--cmsUrl' parameter) - The URL of the CMS
* Token ('--token' parameter) - The authentication token to use. Visit https://www.progress.com/documentation/sitefinity-cms/generate-access-key for instructions on how to generate a token.

### Optional parameters
* Recreate ('--recreate' parameter) - Instructs the command to recreate the selected page/template AND its parent templates. Useful when testing and experimenting with custom configurations/custom widget migrations
* Recursive ('--recursive' parameter) -  Recursively migrates all the child pages/templates of the selected page/template. When migrating templates, the tool does not recursively migrate pages.
* Replace ('--replace' parameter) - Replaces the content of the page. Valid only for pages.
* Action ('--action' parameter) - The action to execute at the end of the migration - Save as Draft/Publish. Allowed values are: draft, publish.

**NOTE** All parameters can be manually specified in the appsettings.json file. **You need to manually create this file next to the sf.exe binary.**
**EXAMPLE**

``` json

{
  "Commands": {
    "Migrate": {
      "CmsUrl": "http://localhost",
      "Token": "TOKEN",
      "Recreate": true
    }
  }
}

```

You can mix both appsettings.json parameters and direct cmd parameters, with the latter having precedence.

### Widget migration
There are two options for migration widgets.
* Through configuration
* Through custom widget migrations

### Migration through configuration
This can be specified in the appsettings.json file.
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

Custom widget migrations can be used when the configuration is not sufficient and more complex logic is required. All OOB widget migrations are located under the Migrations folder for reference.

Widget migrations are invoked for each **occurrence** of the widget found on the page/template. For each invocation a **WidgetMigrationContext** is passed. It contains:
* SourceClient -> The IRestClient for interfacing with the CMS
* Source -> The original widget, from which we are migrating.
* ParentId -> The new parent id.
* Language -> The current language.
* SegmentId -> The page segment (for personalized pages)
* WidgetSegmentId -> The widget segment (for personalized widgets)
* LogWarning -> logs a warning message
* LogInfo -> logs an informational message

A return parameter **MigratedWidget** is required as the output of the migration. It contains the new widget name and the new properties.

**Helpful functions**
From base class MigrationBase

* ProcessProperties -> Copes and renames properties.
* GetMasterIds -> Gets the master ids of the live content items (usually referenced in WebForms widgets).
* GetSingleItemMixedContentValue, GetMixedContentValue -> Helper for generating properties of type MixedContentContext

## Known issues
#### Visual Studio 2015 integration
Sitefinity VSIX/CLI correctly updates the csproj and sln files but Visual Studio 2015 won't refresh the solution correctly.
The workaround is to reopen the solution
