# Sitefinity-CLI

## Installation

### Using installer 
Windows installer(x32/x64) can be downloaded from the releases assets. It will guide you through the setup of the app.

### Using prebuild version
A prebuild version for some operating systems can be downloaded from releases assets. Extract the archive to a folder of your choice and add this folder to the PATH System variable.

### Building the app by yourself
In order to build the CLI you need to install .NET Core. It can be downloaded from [HERE](https://www.microsoft.com/net/download/windows).

Build the application for your OS by running the following command from the project root folder:
```
dotnet publish -c release -r [rid]
```
**NOTE**: Replace **[rid]** with the identifier for your OS. .NET Core RID Catalogue can be found [HERE](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

Example for Windows 10 x64:
```
dotnet publish -c release -r win10-x64
```

Add the following path to the PATH System variable:
```
(project_root_path)\bin\release\netcoreapp2.0\(rid)\publish
```
The CLI is now ready for use!

## How to use

* Open command prompt and navigate to the root of your Sitefinity project (SitefinityWebApp folder).
* Run ```sf``` command. A help will appear describing the available commands and options.

## Available commands

At the moment the only available command is ```add```. It has 4 subcommands:

#### Add new Resource package to current project

```
sf add package "Test resource package"
```

#### Add new Page template to current project

```
sf add pagetemplate "Test page"
```

#### Add new Grid template to current project

```
sf add gridtemplate "Test grid"
```

#### Add new Widget to current project

```
sf add widget "CustomWidget"
```

For more information about the arguments and options for each command run the help option:
```
sf add [command name] -?
```

## Sitefinity version
Every command has an option **"-version"**. It is used to tell the CLI which template version should be used in the generation process. Templates can be found in the _"Templates"_ folder, in separate folders for each sitefinity version (starting from 10.2). 

When running a command the CLI will try to automatically detect Sitefinity version and use the coresponding templates. If it cannot detect the version or Sitefinity version is higher that latest templates version it will be set to the latest available. By using the **"-version"** option it can be explicitly set which templates version should be used.

Example:
```
sf add package "New resource package" --version "11.0"
```
The CLI will look for a folder with name _"11.0"_ inside the _"Templates"_ folder. _"11.0"_ folder has to have _"ResourcePackage"_ folder containing templates for a resource package. 

## Template generation

When running a command the CLI will prompt you for the name of the template to be used in generation. The template name can also be set using the option **"--template"**.

Example:
```
sf add pagetemplate "New page" --template "CustomPageTemplate"
```
The CLI will look for a file _"CustomPageTemplate.Template"_ in the _"Templates\(version)\Page"_ folder. 

### Custom templates

Templates use Handlebars syntax. More about it can be found [HERE](https://github.com/rexm/Handlebars.Net).
Users can easily create custom templates. They should create a file with _".Template"_ extension and place it in the corresponding folder. If the template contains some properties a _"(templateName).config.json"_ file should be created. It must contain all the properties used in the template. The CLI will read the config file and prompt the user to enter the properties when the template is selected.

Example template file:
```
{{> sign}}

{{message}}
{{time}}
{{age}}
```
Example config file:
```json
[
  "message",
  "time",
  "age"
]
```
The partial ```{{> sign}}``` is automatically populated by the CLI.
