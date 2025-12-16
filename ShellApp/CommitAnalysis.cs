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
    public class CommitAnalysis : GitAnalysis
    {
        private CommitStats _stats;
        private TreeHasher? _headTreeHasher = null;

        public CommitAnalysis(bool verbose = false) : base(verbose)
        {
            Verbose = verbose;
        }
        public override GitStatistics Analyze(string repoName, DateTime startTime, AppSettings settings)
        {
            _stats = new CommitStats(repoName);
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
                                    _stats.RegisterSurvival(commit.Author.Name, commit.Author.Email, 1);
                                }

                            }
                        }
                        linesDeleted += entryParentToCommit.LinesDeleted;
                    }
                    parentIdx++;
                }

                _stats.RegisterModification(commit.Author.Name, commit.Author.Email, commit.Author.When.LocalDateTime, linesAdded, linesDeleted);
            }
        }   
    }

    public class TreeHasher
    {
        private readonly Dictionary<string, HashSet<string>> _dict;

        public TreeHasher(Tree tree, AppSettings settings)
        {
            _dict = new Dictionary<string, HashSet<string>>();
            BuildLineHashDictionary(tree, settings);
        }
        /// <summary>
        /// Iterate through a Tree and build a dictionary mapping file paths to line-hash sets.
        /// Paths are normalized to use '/' separators. Lines are normalized before hashing (TrimEnd).
        /// </summary>
        protected void BuildLineHashDictionary(Tree tree, AppSettings settings)
        {
            foreach (var entry in tree)
            {
                if (entry.TargetType == TreeEntryTargetType.Blob)
                {
                    var blob = (Blob)entry.Target;

                    // Skip binary files
                    if (blob.IsBinary) continue;

                    using (var contentStream = blob.GetContentStream())
                    // Enable BOM detection by allowing StreamReader to detect encoding from byte order marks
                    using (var reader = new StreamReader(contentStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                    {
                        var lineHashes = new HashSet<string>();
                        string? line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            string normalized = NormalizeLine(line);
                            if (settings.Statistics.IgnoreComments && normalized.StartsWith("//"))
                                continue;

                            if ( normalized.Length > settings.Statistics.CodeLineMinLength)
                            {
                                string hash = ComputeHash(normalized);
                                lineHashes.Add(hash);
                            }
                        }

                        string key = NormalizePath(entry.Path);
                        _dict[key] = lineHashes;
                    }
                }
                else if (entry.TargetType == TreeEntryTargetType.Tree)
                {
                    // Recurse into subtrees
                    var subtree = (Tree)entry.Target;
                    BuildLineHashDictionary(subtree, settings);
                }
            }
        }

        /// <summary>
        /// Check whether a normalized line exists in the given file path within this tree.
        /// filePath is normalized by converting backslashes to forward slashes.
        /// </summary>
        public bool HasLine(string filePath, string? line)
        {
            if (line == null) return false;

            string key = NormalizePath(filePath);
            if (_dict.TryGetValue(key, out var lineHashes))
            {
                string normalized = NormalizeLine(line);
                string hash = ComputeHash(normalized);
                return lineHashes.Contains(hash);
            }
            return false;
        }

        public static string NormalizeLine(string line)
        {
            // Remove trailing whitespace and normalize any other rules if needed.
            // Do not Trim() because leading whitespace can be significant in many languages.
            return line.Trim();
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path ?? string.Empty;
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Compute a fixed-length hash of a line. Uses MD5.HashData to avoid allocating an MD5 instance per call.
        /// </summary>
        private static string ComputeHash(string input)
        {
            // Guard against null
            input ??= string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
