using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace ShellApp
{
    public class ContributionAnalysis : GitAnalysis
    {
        private ContributionStats _stats;

        public ContributionAnalysis(bool verbose = false) : base(verbose)
        {
            Verbose = verbose;
        }
        public override GitStatistics Analyze(string repoName, DateTime startTime, AppSettings settings)
        {
            _stats = new ContributionStats();
            _stats.Start();
            var repoPath = Path.Combine(settings.Git.WorkingDirectory, repoName);
            
            using (var repo = new Repository(repoPath))
            {
                // Check out the specified branch            
                Branch branch = Commands.Checkout(repo, settings.Git.Branch);

                IterateCommits(repo, branch, startTime, settings, _stats);
                IterateBlameHunks(repo, startTime, settings, _stats);
            }
            _stats.Stop();
            return _stats;
        }
        void IterateCommits(Repository repo, Branch branch, DateTime startTime, AppSettings settings, ContributionStats stats)
        {           
            var commCnt = 0;

            var commMax = branch.Commits.Count();

            foreach (Commit commit in branch.Commits)
            {
                if( commit.Author.When.LocalDateTime < startTime)
                    continue;

                if (Verbose && commCnt++ % 100 == 0)
                    Console.WriteLine($"{(100 * (double)commCnt / commMax).ToString("0.0")}% ({commCnt}/{commMax}) {commit.Author.When.LocalDateTime} {commit.Author} - Commit: {commit.Id} ");

                long linesAdded = 0;
                long linesDeleted = 0;

                int parentIdx = 0;
                if (commit.Parents.Count() > 1) // let's ignore merges
                    continue;

                foreach (var parent in commit.Parents)
                {
                    Patch patch = repo.Diff.Compare<Patch>(parent.Tree, commit.Tree, settings.Statistics.GitExtensionFilter);               

                    foreach (var entry in patch)
                    {
                        foreach (var line in entry.AddedLines)
                        {
                            // only count lines with at least <CodeLineMinLength> characters
                            if (line.Content.Length > settings.Statistics.CodeLineMinLength)
                                linesAdded++;
                        }
                        linesDeleted += entry.LinesDeleted;
                    }
                    parentIdx++;
                }

                stats.RegisterModification(commit.Author.Name, commit.Author.Email, commit.Author.When.LocalDateTime, linesAdded, linesDeleted);
            }
        }

        void IterateBlameHunks(Repository repo, DateTime startDate, AppSettings settings, ContributionStats stats)
        {
            // TODO: respect settings
            // enrich with line survival                

            long fileCnt = 0;
            long fileMax = repo.Index.Count;

            foreach (var filePath in repo.Index)
            {
                if (Verbose && fileCnt++ % 100 == 0)
                    Console.WriteLine($"{(100 * (double)fileCnt / fileMax).ToString("0.0")}% ({fileCnt}/{fileMax})");

                string extension = Path.GetExtension(filePath.Path);
                if (!settings.Statistics.ExtensionsSet.Contains(extension))
                    continue;

                string fullPath = Path.Combine(repo.Info.WorkingDirectory, filePath.Path);
                if (File.Exists(fullPath))
                {
                    var blameHunks = repo.Blame(filePath.Path);
                    var lines = File.ReadAllLines(fullPath);

                    foreach (BlameHunk? hunk in blameHunks)
                    {
                        if (hunk?.FinalSignature.When.LocalDateTime < startDate)
                            continue;

                        int lineCnt = 0;
                        for (int i = hunk.FinalStartLineNumber; i < hunk.FinalStartLineNumber + hunk.LineCount; i++)
                        {
                            // only count lines with at least 5 characters
                            if (lines[i].Length > settings.Statistics.CodeLineMinLength)
                                lineCnt++;
                        }
                        stats.RegisterSurvival(hunk.FinalSignature.Name, hunk.FinalSignature.Email, lineCnt);
                    }
                }
            }
        }
    }
}
