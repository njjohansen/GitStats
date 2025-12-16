// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using ShellApp;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Populate the AppSettings class
var appSettings = new AppSettings();
configuration.Bind(appSettings);

// Get the list of Git repositories
var gitRepositories = configuration.GetSection("GitRepositories").Get<List<string>>();


// Print the repositories
Console.WriteLine($"Known repository names: ");
foreach (var repo in appSettings.Git.Repositories)
{
    var repoName = new Uri(repo).Segments.Last().TrimEnd('/');
    Console.WriteLine($"\t{repoName}");
}
Console.WriteLine($"");

Console.WriteLine($"Known commands: ");
Console.WriteLine($"\tupdate [all | <repository name>] (default: all)");
Console.WriteLine($"\tcount [all | <repository name>] (default: all)");
Console.WriteLine($"\tcontribution [all | <repository name>] (default: all)");
Console.WriteLine($"\tcontribution3m [all | <repository name>] (default: all)");
Console.WriteLine($"\tcommits [all | <repository name>] (default: all)");
Console.WriteLine($"\tcommits3m [all | <repository name>] (default: all)");
Console.WriteLine($"\texit");

bool exit = false;
while (!exit)
{
    Console.Write(": ");
    string input = Console.ReadLine();
    string[] arguments = input.Split(' ').Skip(1).ToArray();
    string command = input.Split(' ').First();
    exit = InvokeCommand(command, arguments);
}

bool InvokeCommand(string command, params string[] args)
{    
    switch (command.ToLower())
    {
        case "exit":
            return true;
        case "update":
            var updateStats = IterateRepositories<EmptyStats>(new UpdateAnalysis(true), DateTime.MinValue, args);
            PrintStats(updateStats);
            break;
        case "count":
            var countStats = IterateRepositories<CountStats>(new CountAnalysis(true), DateTime.MinValue ,args);
            PrintStats(countStats);
            var consolidated = countStats.Consolidate();
            consolidated.PrintFriendly();
            break;
        case "contribution":
            var contribStats = IterateRepositories<ContributionStats>(new ContributionAnalysis(true), DateTime.MinValue, args);
            PrintStats(contribStats);
            break;
        case "contribution3m":
            var contribStats3m = IterateRepositories<ContributionStats>(new ContributionAnalysis(true), DateTime.Now.AddMonths(-3), args);
            PrintStats(contribStats3m);
            break;
        case "commits":
            var commitStats = IterateRepositories<CommitStats>(new CommitAnalysis(true), DateTime.MinValue, args);
            PrintStats(commitStats);
            break;
        case "commits3m":
            var commitStats3m = IterateRepositories<CommitStats>(new CommitAnalysis(true), DateTime.Now.AddMonths(-3), args);
            PrintStats(commitStats3m);
            break;
        default: 
            Console.WriteLine("Unknown command");
            break;

    }
    return false;
}

void PrintStats<T>(GitStatisticsList<T> stats) where T: GitStatistics, new()
{
    foreach (var stat in stats)
    {
        stat.PrintFriendly();
    }
}

GitStatisticsList<T> IterateRepositories<T>(GitAnalysis analysis, DateTime startTime, params string[] args) where T: GitStatistics, new()
{
    var stats = new GitStatisticsList<T>();
    List<string> repoNames = new List<string>();
    if (args.Length == 0 || args[0].ToLower() == "all")
    {
        repoNames = appSettings.Git.Repositories.Select(r => new Uri(r).Segments.Last().TrimEnd('/')).ToList();
    }
    else
    {
        repoNames.AddRange(args);
    }

    foreach (var repoName in repoNames)
    {
        Console.WriteLine($"Analyzing repository '{repoName}'...");
        stats.Add((T) analysis.Analyze(repoName, startTime, appSettings));        
    }    
    
    return stats;
}


Console.ReadLine();
