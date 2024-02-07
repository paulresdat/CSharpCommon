# CSharpCommon

![Supported .NET8](https://img.shields.io/badge/Supported-.NET8-blue)
[![Build Status](https://dev.azure.com/paulcarlton/Csharp.Common/_apis/build/status%2Fpaulresdat.CSharpCommon?branchName=main)](https://dev.azure.com/paulcarlton/Csharp.Common/_build/latest?definitionId=1&branchName=main)


![Supported .NET7](https://img.shields.io/badge/Supported-.NET7-blue)
[![Build Status](https://dev.azure.com/paulcarlton/Csharp.Common/_apis/build/status%2Fpaulresdat.CSharpCommon?branchName=net7)](https://dev.azure.com/paulcarlton/Csharp.Common/_build/latest?definitionId=1&branchName=net7)


![Not Supported .NET6](https://img.shields.io/badge/Unsupported-.NET6-red)

A C# common library for bootstrapping and jump starting projects of any framework (on .NET).


## Audience

The intended audience are folks who are designing applications or have experience in starting up console/web apps from scratch.  Being knowledgable of the following is recommended:

1. .NET 6 / 7 & 8 - Preferrably all 3 but at least 7 or 8 is best or a deep understanding of 6 if you haven't gotten there yet.
2. An understanding of Onion Architecture and general programming design principles like SOLID as well as object design principles.  Read the books "Design Patterns" and "Clean Architecture" if you haven't already.


## Purpose

Spend less time on the infrastructure and more time on the project's code.  We're reinventing the wheel all the time for lower level handling and this project tries to minimize some of that by offering some often used algorithms close to .NET standards and recommended code conventions.


## Style

This project adheres to the .NET standard code styling for the project found here: https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md

If code is contributed without adhering to these code style conventions, you'll be asked to change it before it is accepted.


## Clean Code

This project emphasizes "Clean Code" conventions.  There's a lot of boiler plate code handled, particularly in unit/integration testing projects, that this project helps to address.  We're trying to clean up the eye sore that can evolve with complex mocking and transaction based testing on databases.


## Goals

For now the goal is to provide a good project starter codebase that helps me stay productive.  I know for projects that I work on, this helps me get stuff done in a relatively short period of time without having to think too much about how I'll work with .NET itself to get something done.

This project has been used extensively in both a console app context as well as the new Blazor application framework.  The algorithms used are universal enough to be utilized in any .NET project besides those aforementioned, including Maui and of course MVC.

I'd like this to turn into a NuGet package, but I haven't thought that far ahead on this and haven't devoted enough time to this project for planning and logistics around that goal.  Maybe some day.

## Installing

Since this project is not a NuGet package, you'll have to pull down the project as a git submodule.  You are welcome to customize it if you like it.  Just put in a pull request into master and I'll figure out what needs to be done with it and how it works with the rest of the project.

The other way is to just download it and copy over the project folders you need the most in order of their precedence outlined below.  For instance, you have to have the common library to also loop in the entityframework library.

## Contributing

You can contribute to the master branch by putting in a Pull Request.  If the change helps the project, I'll try and apply it to both the latest .NET versions the project targets.

## Project Structure

The below tree outlines the relation of the projects to each other.  There is the parent project and the child projects that are references to that parent project.

1. Csharp.Common
    - No references
1. Csharp.Common.Unsafe
    - No references
1. Csharp.Common.UnitTesting
    1. Csharp.Common
1. Csharp.Common.EntityFramework
    1. Csharp.Common
1. Csharp.Common.EntityFramework.UnitTesting
    1. Csharp.Common.UnitTesting

The project `Csharp.Common.UnitTests` is the unit test project for the library itself and does not need to be a part of any of your solutions directly.

When using the project, it's important to link in the projects that are related if you're going to be using one of the projects besides the Csharp.Common library.  In your solution, don't forget to include the referenced project so that your builds build properly.  Restructuring may hide broken project links since they were previously compiled and may cause issues in the future.