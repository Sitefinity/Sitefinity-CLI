# Contributing to Sitefinity CLI

## Before You Start

Anyone wishing to contribute to the Sitefinity CLI project MUST read & sign the [Sitefinity CLI Contribution License Agreement](https://progress.co1.qualtrics.com/jfe/form/SV_8tPGdyWEdKUg5cF). The Sitefinity CMS team cannot accept pull requests from users who have not signed the CLA first.

## Introduction

These guidelines are here to facilitate your contribution and streamline the process of getting changes merged into this project and released. Any contributions you can make will help tremendously, even if only in the form of an issue report. Following these guidelines will help to streamline the pull request and change submission process.

## Report an Issue

If you find a bug in the source code or a mistake in the documentation, you can submit an issue to our [GitHub Repository](https://github.com/Sitefinity/Sitefinity-CLI).
Before you submit your issue, search the archive to check if a similar issues has been logged or addressed. This will let us focus on fixing issues and adding new features.
If your issue appears to be a bug, and hasn't been reported, open a new issue. To help us investigate your issue and respond in a timely manner, you can provide is with the following details.

* **Overview of the issue:** Provide a short description of the visible symptoms. If applicable, include error messages, screen shots, and stack traces.
* **Motivation for or use case:** Let us know how this particular issue affects your work.
* **Sitefinity CLI version:** Always update to the most recent master release; the bug may already be resolved.
* **Steps to reproduce:** If applicable, submit a step-by-step walkthrough of how to reproduce the issue.
* **Related issues:** If you discover a similar issue in our archive, give us a heads up - it might help us identify the culprit.
* **Suggest a fix:** You are welcome to suggest a bug fix or pinpoint the line of code or the commit that you believe has introduced the issue.

## Requesting New Features

You can request a new feature by submitting an issue to our GitHub Repository or visit the [Sitefinity Feedback portal](https://feedback.telerik.com/Project/153), and search this list for similar feature requests.

## Code Fixes and Enhancements

### 1. Log an Issue

Before doing anything else, we ask that you file an issue in the Issues list for this project. First, be sure to check the list to ensure that your issue hasn't already been logged. If you're free and clear, file an issue and provide a detailed description of the bug or feature you're interested in. If you're also planning to work on the issue you're creating, let us know so that we can help and provide feedback.

### 2. Fork and Branch

#### Install dependancies
[Sitefinity CLI.Installer](https://github.com/Sitefinity/Sitefinity-CLI/tree/master/Sitefinity%20CLI.Installer) is a WIX project, make  sure Visual Studio 2017 has the [WIX extension](https://marketplace.visualstudio.com/items?itemName=RobMensching.WixToolsetVisualStudio2017Extension) installed 

#### Fork Us, Then Create A Topic Branch For Your Work

The work you are doing for your pull request should not be done in the master branch of your forked repository. Create a topic branch for your work. This allows you to isolate the work you are doing from other changes that may be happening.

Github is a smart system, too. If you submit a pull request from a topic branch and we ask you to fix something, pushing a change to your topic branch will automatically update the pull request.

#### Isolate Your Changes For The Pull Request

See the previous item on creating a topic branch.

If you don't use a topic branch, we may ask you to re-do your pull request on a topic branch. If your pull request contains commits or other changes that are not related to the pull request, we will ask you to re-do your pull request.

### 3. Include tests describing the bug or feature

If possible you should add a test validating the code changes. You may browse the `Sitefinity CLI.Tests` project to get a better idea of the structure and conventions used.

To run the tests, execute the following command:

```
dotnet test
```

#### (optional) Squash your commits

When you've completed your work on a topic branch, you may squash your work down into fewer commits to make the merge process easier. For information on squashing via an interactive rebase, see [the rebase documentation on GitHub](https://help.github.com/articles/interactive-rebase)

### 3. Submit a Pull Request

See [Github's documentation for pull requests](https://help.github.com/articles/using-pull-requests).

Pull requests are the preferred way to contribute to Sitefinity CLI. Any time you can send us a pull request with the changes that you want, we will have an easier time seeing what you are trying to do. It is very important to provide a meaningful description with your pull requests that alter any code.
