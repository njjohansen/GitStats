# ADO Git Statistics

The console application let you easily get statistics about your git repositories. 

It is a great tool for developers who want to analyze their codebase and understand their development patterns.

## Features
It connects to Azure DevOps can automatically fetch all repositories with the "pull" command.

Having the repositories fetched, the following commands to build statistics: 
* count - counts the number of commits, contributions, and contributors for each repository and for the configured file types.
* countdate - same as "count" but on the last back before or on the given date. 
* contribution - calculates the contribution of each contributor to the repository based on the number of commits and the number of survived lines at head.
* contribution3m - same as "contribution" but limited to commits from the last 3 months.
* commits - lists all commits for each repository and for the configured file types.
* commits3m - same as "commits" but limited to commits from the last 3 months.
* delta - count lines per file extension type at specified date and at head, and analyze commits between the two dates to calculate the number of surviving lines added in the period.

## Installation
Clone the repository and build the project with your favorite build tool (e.g. Visual Studio, dotnet CLI, etc.).
