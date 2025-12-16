# Sitefinity CLI

## Overview

You use Sitefinity CLI to perform maintainance tasks on your Sitefinity project, such as creating a new project, adding resources to an existing project, updating the project to a newer version, or migrating the front-end rendering framework.

## Prerequisites

  To use or build the CLI, you need to install the corresponding version of the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
  Some CLI operations require a supported version of Visual Studio to be installed and configured for Sitefinity CMS development. For more information, see [Install Sitefinity CMS](https://www.progress.com/documentation/sitefinity-cms/install-sitefinity).

## Installation

* Use prebuild version

  You can download a prebuild version for some operating systems from the [release assets](https://github.com/Sitefinity/Sitefinity-CLI/releases). Extract the archive to a folder of your choice and add this folder to the ```PATH``` system variable.

## Build the app yourself

  To build the application for your OS, enter the following command from the project root folder:

  ```dotnet publish -c release -r [rid]```

  Replace [rid] with the identifier for your OS. For more information, see the [.NET Core RID Catalogue](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

  **IMPORTANT**: Progress supports the Sitefinity CLI only as Windows x64 build.

  **EXAMPLE**: To build the app for Windows x64, enter the following command:

  ```dotnet publish -c release -r win-x64```

  and add the following path to the PATH System variable:

  ```(project_root_path)\bin\Release\(SDKver)\(rid)\publish```

## How to use

1. Open command prompt and navigate to the root of your Sitefinity project (`SitefinityWebApp` folder).
1. Run ```sf``` command.

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
  
  Add ```--renderer``` to the command to create a new ASP.NET Core renderer project for Sitefinity.

  Add ```--use-sln``` to the command to use .sln solution type instead of the default one .slnx type.

  **NOTE**: After creating the renderer project, you have to set your Sitefinity CMS url in the *appsettings.json* and update *launchSettings.json*. For more information, see [Configure the ASP.NET Core Renderer](https://www.progress.com/documentation/sitefinity-cms/install-sitefinity-in-.net-core-mode-dp#configure-the-asp-net-core-renderer).
  

  Run the help option to see all available install options and configurations.

* To **Upgrade** your project, execute the following command:
  * **For .sln**: ```sf upgrade "(path-to-your-project)\SitefinityWebApp.sln" "15.3.8500"```

  * **For .slnx**: ```sf upgrade "(path-to-your-project)\SitefinityWebApp.slnx" "15.3.8500"```

  Add ```--removeDeprecatedPackages``` to the command to automatically remove deprecated packages during the upgrade process to ensure a clean and up-to-date codebase.

  For more information, see [Upgrade using Sitefinity CLI](https://www.progress.com/documentation/sitefinity-cms/upgrade-using-sitefinity-cli).

* To show the version of Sitefinity CLI, execute the following command:

  ```sf version```

**NOTE**: For more information about the arguments and options for each command, run the help option. For example:

```sf add [command name] -?```

### Migration commands

You use the migration commands to migrate front-end resources to a decoupled renderer architecture based on ASP.NET Core or Next.js.

These include page templates, pages, built-in widgets, forms, and form responses - written in Web Forms or MVC. You use Sitefinity CLI to migrate a top-level resource - a page template or a page. Afterwards, the CLI automatically migrates the resources used on it, such as widgets, and forms.

Sitefinity CLI has the following commands:

* ```sf migrate page "PageId"```<br>
  Migrates a page.

* ```sf migrate template "TemplateId"```<br>
  Migrates a page template.

* ```sf migrate responses "FormId"```<br>
  Migrates the form responses for the form with the specified `FormId`.

* ```sf migrate --help```<br>
  Prints online the same help as this article.

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

You use the migration commands to migrate frontend resources to a decoupled renderer architecture based on ASP.NET Core or Next.js.

These include page templates, pages, built-in widgets, forms, and form responses - written in Web Forms or MVC. You use Sitefinity CLI to migrate a top-level resource - a page template or a page. Afterwards, the CLI automatically migrates the resources used on it, such as widgets, and forms.

**NOTE**: The `PageId`, `TemplateId`, and `FormId` are the internal GUIDs that Sitefinity CMS uses to identify the respective frontend resource. You can use the Sitefinity Migration Analyzer to conveniently get these GUIDs. For more information, see [Sitefinity Migration Analyzer](https://www.progress.com/documentation/sitefinity-cms/sitefinity-migration-analyzer).

<div>
**IMPORTANT**: The Sitefinity CLI tool helps you migrate only  the following frontend resources:

* the content and structure of the page templates and pages.
* built-in widgets, if they have equivalent in the decoupled renderers.<br>
  For more information, see [Widgets](https://www.progress.com/documentation/sitefinity-cms/widgets-site-components).
* form responses

You still need to re-implement all custom widgets you are using on your site.<br>
The migration tool is not a complete solution and can generate warnings or incomplete front-end resources.<br>
You are responsible to check its results.
</div>

**IMPORTANT**: The `migrate` command supports migration from Web Forms and MVC widgets to ASP.NET Core and Next.js widgets only.

**PREREQUISITES**: The migration commands support only projects based on Sitefinity 15.3 and later.<br>
If you want to migrate a project based on an earlier Sitefinity CMS version, you need to first upgrade your project to at least Sitefinity 15.3.

### General flow of migration

**RECOMMENDATION**: We recommend analyzing and evaluating the state of your Sitefinity project and estimating the resources needed for migration before starting the migration itself.<br>
For more information, see [Technology migration](https://www.progress.com/documentation/sitefinity-cms/technology-migration).

To migrate your Sitefinity CMS project, perform the following procedure:

1. Migrate the templates.<br>
  Start with the migration of templates that a subset of pages is based on OR migrate all of the page templates at once.
1. Adjust the migrated structure as needed:
    * Set a file system template.
    * Migrate the widgets used on the template
      * Take a business decision whether you can stop using some of the existing widgets. For example, remove existing widgets if the busness need is no longer there.
      * Manually configure the widgets that are migrated.
    * Adjust the look and feel of the page.
1. Once you complete the migration of the chosen page templates, move on to the pages structure.
    * Sitefinity CLI duplicates the Pages by default and excludes the copies from the navigation.
    * Once you fully migrate the page, specify the `--replace` option to the CLI `migrate page` command.
1. Migrate the forms.<br>
  For each form you are using, perform the following:
    * Complete the migration of all pages and page templates where you are using this form in one step.
    * Ensure that only the migrated form is used on the frontend and the initial form is hidden from all MVC and Web Forms resources.
    * Run the CLI command `migrate responses` to migrate form responses.

**IMPORTANT**: Sitefinity CLI migrates fully all form responses every time you run the command. Therefore, if you run the command multiple times, Sitefinity CLI will create duplicated form responses.
To avoid duplications, delete the common responses before running the `migrate responses` command for second time.

#### Considerations

* You can specify the `--replace` option only when the page has a duplicate.
* The CLI uses the page's `UrlName` property OR the page template's `Name` property to identify the page or the page template that it created. The format has a suffix of `(migrated to Decoupled)`. This process avoids conflicts with existing pages and page templates.
* The CLI uses only data from the published pages and page templates. `Draft` and `Temp` changes are not migrated.
* Sitefinity CLI automatically migrates forms, if there is a form widget on the page.

### Migrating hierarchies

* When you select a page template, the CLI migrates first the parent page templates. Migration cannot happen otherwise.
* If parent page templates are migrated automatically, they will be published automatically.

**NOTE**: When migrating pages, the CLI will not migrate the parent page templates. You must migrate all dependent page templates before migrating pages.

### Safe box & Testing

All pages and page templates are duplicated by default with a suffix `(migrated to Decoupled)` appended to their `Title`. This provides a level of isolation for existing pages and page templates, so that the migration can happen seamlessly and without downtime. This is a useful way to test the changes before they go live.

**NOTE**: When migrating pages, you can use the option `--replace`. This option replaces the page contents on the **actual** page and saves the page automatically as a `Draft`, regardless of the value of the `--action` option.<br>
This option keeps the existing links from content blocks, HTML fields, and related data; thus, you do not need to update these references.

**NOTE**: Duplicated Pages are hidden from navigation by default.

### CLI command-line parameters

#### Required parameters

The following CLI parameters are required:

* `--cmsUrl`<br>
  The URL of the deployed Sitefinity CMS.<br>
  For example, `https://www.example.com/en`
* `--token`<br>
  The authentication token to use.<br>
  To learn how to generate a token, see [Generate access key](https://www.progress.com/documentation/sitefinity-cms/generate-access-key). You must use a token of an account with full _Administrator_ privileges.

#### Optional parameters

The following CLI paramaters are optional:

* `--framework`<br>
  Specifies the target renderer framework. Valid only for page templates and forms. The accepted values are `NetCore` for ASP.NET Core and `NextJS` for Next.js. If you omit this option, the default ASP.NET Core renderer is used.
* `--recreate`<br>
  Recreates the selected page or template **and** its parent templates.<br>
  Useful when testing and experimenting with custom configurations/custom widget migrations
* `--recursive`<br>
  Recursively migrates all the child pages or templates of the selected page/template. When migrating templates, the tool does not recursively migrate pages.
* `--replace`<br>
  Replaces the content of the page. Valid only for pages.
* `--action`<br>
  The action to execute at the end of the migration. Allowed values are:
  * `draft` - saves the migrated resource as a `Draft`.
  * `publish` - publishes the migrated resource.
* `--siteid`<br>
  The site id. You use the `--siteid` parameter to specify the site id when you work with a site different from the default site.

**NOTE**: You can set the parameters manually in the appsettings.json file. You need to manually create the appsettings.json file next to the _sf.exe_ binary.<br>
You can mix both appsettings.json parameters and direct command-line parameters, with the latter having precedence.

**EXAMPLE**: Following is an example appsettings.json file.
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

### Widget migration
You can migrate widgets in two ways - using configuration and using custom widget migrations.

### Migration through configuration
You specify the migrations in the appsettings.json file.

**EXAMPLE**:
``` json

{
  "Commands": {
    "Migrate": {
      ...
      "Widgets": {
          "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock": {
              // the name of the new widget in NetCore/NextJs renderers
              "Name": "SitefinityContentBlock",
              // the whitelist of properties to keep during the migration
              "Whitelist": ["Html", "ProviderName", "SharedContentID"],
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

You can perform custom widget migrations when the configuration using the CLI is insufficient and a more complex logic is required. You use C# code to extend the functionality of the CLI itself.<br>
You can reference the built-in widget migrations, located under the `Migrations` folder in the CLI source code.

Widget migrations are invoked for each **occurrence** of the widget found on the page or page template. For each invocation, a `WidgetMigrationContext` is passed. It contains:

* `SourceClient`
  The `IRestClient` for interfacing with the CMS.
* `Source`
  The original widget, from which you are migrating.
* `ParentId`
  The new parent id for the migrated widget.
* `Language`
  The current language.
* `SegmentId`
  The page segment when working with personalized pages.
* `WidgetSegmentId`
  The widget segment when working with personalized widgets.
* `LogWarning`
  Specifies a warning message to be shown in the Terminal during the migration of the widget.
* `LogInfo`
  Specifies an informational message to be shown in the Terminal during the migration of the widget.
* `SiteId`
  The site id. You use the `SiteId` parameter to specify the site id when you work with a non-default site.

A return value of type `MigratedWidget` is required as the output of the migration. It contains the new widget name and properties.

#### Useful functions

From base class `MigrationBase`:

* `ProcessProperties`
  Copes and renames properties.
* `GetMasterIds`
  Gets the master ids of the live content items (usually referenced in Web Forms widgets).
* `GetSingleItemMixedContentValue`, `GetMixedContentValue`
   Helper for generating properties of type `MixedContentContext`.

### Map form fields when migrating form responses

When migrating forms responses, you may want to change the type of fields you are migrating.<br>
For example, you may want to migrate a custom field to a built-in one, or migrate a built-in one, which is no longer supported, to a custom one.<br>
To do this, you create a mapping configuration in the sf.exe's `appsettings.json` file.<br>
In the `FormFieldNameMap` object, you create a name/value pair, containing the names of the developer types of the respective form fields.<br>
For example, to map a MVC text form field to an ASP.NET Core form field, the configuration should look like:

```json
"FormFieldNameMap": {
  "sf_contactus.TextFieldController": "SitefinityTextFieldFormControl"
}
```

You construct the JSON key using the name of the source form (the one you migrate from), DOT, and the name of the form field. The JSON value is the name of the target form field (the one you migrate to).<br>
In the Migration Analyzer, you navigate to the form you want to migrate. You take the form name from the _'Form name' form info_ table, under the _Form name_ column, and the field name from the _Fields used in this form_ table under the _Name (used in code)_ column.<br>
You can take both names from the Sitefinity Migration Analyzer. For more information see [Sitefinity Migration Analyzer Forms](https://www.progress.com/documentation/sitefinity-cms/sitefinity-migration-analyzer-forms).

**EXAMPLE**:

```json
{
  "Commands": {
    "Migrate": {
      "CmsUrl": "https://yoursitefinityinstance.net",
      "Token": "authentication-token",
      "PlaceholderMap": {
        "Contentplaceholder1": "Body"
      },
      "Widgets": {
        "Telerik.Sitefinity.Modules.GenericContent.Web.UI.ContentBlock": {
          "Name": "SitefinityContentBlock", // the name of the new widget in NetCore/NextJs renderers
          "Whitelist": [ "Html", "ProviderName", "SharedContentID" ], // the whitelist of properties to keep during the migration
          "Rename": { // the properties to be renamed
            "Html": "Content"
          },
          "CleanNullProperties": true
        },
        "Telerik.Sitefinity.Frontend.Forms.Mvc.Controllers.CaptchaController": {
          "Name": "Captcha2", // the name of the new widget in NetCore/NextJs renderers
          "Whitelist": [], // the whitelist of properties to keep during the migration
          "Rename": {
          },
          "CleanNullProperties": true
        }
      },
      "FormFieldNameMap": {
          "sf_contactus.TextFieldController": "SitefinityTextFieldFormControl"
      }
    }
  }
}
```

### Limitations

The CLI migration command does not migrate the code in any form. It can migrate only the content and structure of your project.

#### Migrating List widget to Content list widget

When you migrate a List widget ([Web Forms](https://www.progress.com/documentation/sitefinity-cms/133/list-widget-webforms), [MVC](https://www.progress.com/documentation/sitefinity-cms/list-widget-mvc)) to a Content list widget ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)), and the existing widget shows two separate lists, the new Content list will show a single view, containing merged items from all original lists.

To show the multiple original lists separately, perform one of the following:

* create a new view to perform custom sorting and iterate the lists separately
* use multiple separate Content List widgets, each configured to display only one of the lists.

#### Migrating Web Forms Image widget to Image widget

When you migrate a [Web Forms Image widget](https://www.progress.com/documentation/sitefinity-cms/133/image-widget-webforms) with display mode set to `Custom`, the custom display setting is not migrated.

To work around, manually configure the image size through the widget designer.

#### Migrating Dynamic widget to Content list widget

When migrating a [Dynamic content widget](https://www.progress.com/documentation/sitefinity-cms/dynamic-content-widgets) to a Content list ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)) and the main field is changed to a value different from the one in the `Title` field, you need to perform the following:

1. Manually change the list mapping in the widget designer
2. Update the mapping for the `Details` view.<br>
  For ASP.NET Core, you use the `ViewsMetadata.json` file.
  For more information, see [Create custom views for the Content list widget](https://www.progress.com/documentation/sitefinity-cms/create-custom-views-for-the-content-list-widget) » *Field Mappings*.
  For Next.js, you modify the `ListFieldMapping` property. For more information, see [Extend the Content list widget](https://www.progress.com/documentation/sitefinity-cms/next.js-extend-content-list-widget).

#### Migrating Blogs to Content list

Sitefinity CLI migration command does not support migrating Blogs widget ([Web Forms](https://www.progress.com/documentation/sitefinity-cms/133/blogs-list-widget-webforms), [MVC](https://www.progress.com/documentation/sitefinity-cms/blogs-widget-mvc)).

You need to implement a custom widget in ASP.NET Core and manually migrate. We recommend using the Content List widget ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)) to implement your custom widget.

#### Migrate Events widget to Content list widget

The new Content list widget ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)) comes with different predefined basic filter options than the MVC and Web Forms widgets.

Migration process transfers the old filters for past, current, and upcoming events into the *Filter Expression* field in Advanced mode of the widget designer. 

Because of the difference in the filtering functionality between the different widgets, after the migration, you need to check the filters directly in the *Filter Expression* field. 

If the filters expressions cannot be matched, you need to implement a custom widget in ASP.NET Core and manually migrate using this new widget. We recommend using the Content List widget ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)) to implement your custom widget.

#### Migrate Calendar widget

Sitefinity CLI cannot migrate the Calendar widget.

You need to create a custom widget, based on Content List widget ([ASP.NET](https://www.progress.com/documentation/sitefinity-cms/content-list-widget-core), [Next.js](https://www.progress.com/documentation/sitefinity-cms/next.js-content-list-widget)) to implement your custom widget.

#### Migrate Login form with Reset password mode set to Reset password widget

If you have configured the Login form widget ([Web Forms](https://www.progress.com/documentation/sitefinity-cms/133/login-widget-webforms), [MVC](https://www.progress.com/documentation/sitefinity-cms/login-form-widget-mvc)) with `Allow users to reset password` to ASP.NET Core renderer, it is migrated without the option to reset the password because there is a separate widget providing that functionality.

You need to manually add the new Reset password widget ([ASP.NET Core](https://www.progress.com/documentation/sitefinity-cms/reset-password-widget), [Next.js](https://www.progress.com/documentation/sitefinity-cms/nextjs-reset-password-widget)) on the page and configure it.

#### Replacement of multilingual pages

The following scenario is not supported:

1. You migrate a page
1. You add new languages only to the migrated copy of the page
1. You attempt to migrate the page again, using the `--replace` option

In this scenario, only the single original language of the original page is preserved.

#### Migrating form responses

Sitefinity CLI gives you the flexibility to work on the original and migrated pages at the same time.
If the page that you are migrating has a form, and you migrate the form responses while you have the same form on the two variants of the page (using the original and the target frameworks), then Sitefinity CLI will duplicate all form responses for the old and new form.
To avoid duplications, delete the entries from the old form before you run the migrate responses command for a second time.

**RECOMMENDATION**: We recommend that you complete the page (or page template) migration first, hide the original form, based on MVC or Web Forms; and only then migrate the form responses for the forms on that page (or page template).

## Known issues

### Visual Studio 2015 integration
Visual Studio 2015 won't refresh the solution correctly after the Sitefinity VSIX/CLI correctly update the .csproj and .sln files.
To work around the issue, reopen the solution after the CLI modifies it.
