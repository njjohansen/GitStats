using LibGit2Sharp;

namespace ShellApp
{
    public class CountAnalysis : GitAnalysis
    {        
        private CountStats _stats;

        public CountAnalysis(bool verbose = false): base(verbose)
        {
            Verbose = verbose;
        }
        public override GitStatistics Analyze(string repoName, DateTime startTime, AppSettings settings)
        {
            _stats = new CountStats(repoName, Verbose);
            _stats.Start();
            var repoPath = Path.Combine(settings.Git.WorkingDirectory, repoName);            
            using (var repo = new Repository(repoPath))                
            {
                // Check out the specified branch            
                Branch branch = Commands.Checkout(repo, settings.Git.Branch);

                // Find the last commit on the given day (startTime.Date). If none, use the last commit before startTime.
                Commit targetCommit = null;
                var commits = branch.Commits.OrderBy(c => c.Author.When.LocalDateTime).ToList();

                // Try to find commits on the same day
                var lastOnDay = commits
                    .Where(c => c.Author.When.LocalDateTime.Date == startTime.Date)
                    .OrderByDescending(c => c.Author.When.LocalDateTime)
                    .FirstOrDefault();

                if (lastOnDay != null)
                {
                    targetCommit = lastOnDay;
                }
                else
                {
                    // Fallback: last commit at or before startTime
                    var lastBefore = commits
                        .Where(c => c.Author.When.LocalDateTime <= startTime)
                        .OrderByDescending(c => c.Author.When.LocalDateTime)
                        .FirstOrDefault();

                    targetCommit = lastBefore ?? branch.Tip;
                }
                Console.WriteLine($"Counting lines on tree from commit (Author: , {targetCommit.Author.Name}, Date: {targetCommit.Author.When}).");
                // Checkout the target commit (detached HEAD) so the working tree matches that commit
                Commands.Checkout(repo, targetCommit);

                CountLinesInTree(repo, targetCommit.Tree, settings);
            }
            _stats.Stop();
            return _stats;
        }

        void CountLinesInTree(Repository repo, Tree tree, AppSettings settings)
        {                 
            foreach (var entry in tree)
            {
                int lineCount = 0;
                if (entry.TargetType == TreeEntryTargetType.Blob)
                {
                    string extension = Path.GetExtension(entry.Name).ToLowerInvariant();
                    if (!settings.Statistics.ExtensionsSet.Contains(extension))
                        continue;
                    var blob = (Blob)entry.Target;
                    using (var contentStream = blob.GetContentStream())
                    using (var reader = new StreamReader(contentStream))
                    {                         
                        while (reader.ReadLine() != null)
                        {
                            lineCount++;
                        }
                    }
                    _stats.Add(extension, lineCount);
                }
                else if (entry.TargetType == TreeEntryTargetType.Tree)
                {
                    CountLinesInTree(repo, (Tree)entry.Target, settings);
                }

            }

        }
    }
}
