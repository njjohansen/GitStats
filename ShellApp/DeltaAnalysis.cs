using LibGit2Sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ShellApp
{
    public class DeltaAnalysis : GitAnalysis
    {
        private CountStats _stats;
        private TreeHasher? _headTreeHasher = null;

        public DeltaAnalysis(bool verbose = false) : base(verbose)
        {
            Verbose = verbose;
        }
        public override GitStatistics Analyze(string repoName, DateTime startTime, AppSettings settings)
        {
            _stats = new CountStats(repoName);
            _stats.Start();
            var repoPath = Path.Combine(settings.Git.WorkingDirectory, repoName);
            
            using (var repo = new Repository(repoPath))
            {
                // Check out the specified branch            
                Branch branch = Commands.Checkout(repo, settings.Git.Branch);
                _headTreeHasher = new TreeHasher(branch.Tip.Tree, settings);

                IterateCommits(repo, branch, startTime, settings);                
            }
            _stats.Stop();
            return _stats;
        }
        private void IterateCommits(Repository repo, Branch branch, DateTime startTime, AppSettings settings)
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
                    Patch patchParentToCommit = repo.Diff.Compare<Patch>(parent.Tree, commit.Tree, settings.Statistics.GitExtensionFilter);

                    foreach (var entryParentToCommit in patchParentToCommit)
                    {

                        string extension = Path.GetExtension(entryParentToCommit.Path).ToLowerInvariant();
                        if (!settings.Statistics.ExtensionsSet.Contains(extension))
                            continue;

                        foreach (var line in entryParentToCommit.AddedLines)
                        {
                            // only count lines with at least <CodeLineMinLength> characters
                            var lineStr = TreeHasher.NormalizeLine(line.Content);
                            if( settings.Statistics.IgnoreComments && lineStr.StartsWith("//"))
                            {
                                continue;
                            }
                            if (lineStr.Length > settings.Statistics.CodeLineMinLength)
                            {
                                linesAdded++;
                                if (_headTreeHasher.HasLine(entryParentToCommit.Path, lineStr))
                                {
                                    //Console.Write($"Survived Line in Commit {commit.Id}: {line.Content}");
                                    _stats.Add(extension, 1);
                                }

                            }
                        }
                        linesDeleted += entryParentToCommit.LinesDeleted;
                    }
                    parentIdx++;
                }                
            }
        }   
    }
}
