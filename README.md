# Sitefinity CLI

## Installation

* Use the installer
  
  Download the Windows installer(x32/x64) from the [release assets](https://github.com/Sitefinity/Sitefinity-CLI/releases) and follow the steps to setup the app.

* Use prebuild version

  You can download a prebuild version for some operating systems from the [release assets](https://github.com/Sitefinity/Sitefinity-CLI/releases). Extract the archive to a folder of your choice and add this folder to the ```PATH``` system variable.

## Build the app yourself

  **PREREQUISITES**: To build the CLI, you need to install [.NET Core](https://www.microsoft.com/net/download/windows).
  
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

  ```sf add tests "Sitefinity.Tests.Integration" -t Default```
  
  or
  
  ```sf add tests "Sitefinity.Tests.Integration" -t Sample```
  
* To create a new **Custom Module**, execute the following command:

  ```sf add module "Custom Module" -d "Description of the custom module"```

**NOTE**: For more information about the arguments and options for each command, run the help option:

```sf add [command name] -?```

## Sitefinity CMS version

Every command has an option ```-version```. It is used to tell the CLI which template version should be used in the generation process. Templates can be found in the ```Telates``` folder, in separate folders for each Sitefinity CMS version, starting from 10.2.

When running a command the CLI will try to automatically detect your Sitefinity CMS project version and use the corresponding template. If it cannot detect the version or your Sitefinity CMS version is higher than latest templates version, CLI will use the latest available. 

You can use the ```-version``` option to explicitly set the templates version that CLI should use.

**EXAMPLE**: Following is an example command of using the ```–version``` option:
```
sf add package "New resource package" --version "11.0"
```
In this case, the CLI will look for a folder named ```11.0``` inside folder ```Templates```. Folder 11.0 must have ```ResourcePackage``` folder containing templates for a resource package.

## Template generation

When you run a command, the CLI prompts you to enter the name of the template to be used for the generation. You can also set the name using option ```-template```.

**EXAMPLE**: Following is an example command of using the –template option:
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

## Known issues
#### Visual Studio freeze/crash bug
When Sitefinity VSIX is installed for the first time or there is a newer version of Sitefinity CLI, Visual Studio freezes until CLI is downloaded and unzipped and in some cases crashes. 

After Sitefinity CLI is downloaded and unzipped Visual Studio restores it's normal behaviour
