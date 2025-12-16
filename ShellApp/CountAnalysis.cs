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
                Commit lastComit = branch.Tip;
                CountLinesInTree(repo, lastComit.Tree, settings);
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
