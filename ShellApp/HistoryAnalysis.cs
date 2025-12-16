//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Xml.Linq;
//using LibGit2Sharp;

//namespace ShellApp
//{
//    public class HistoryAnalysis : GitAnalysis
//    {
//        private bool Verbose { get; }
//        private HistoryStats _stats = new HistoryStats(true);

//        public HistoryAnalysis(bool verbose = false)
//        {
//            Verbose = verbose;
//        }
//        public GitStatistics Analyze()
//        {
//            string repoPath = @"C:\git-aula\web-app";
//            string branchName = "develop"; // Specify your branch name here
                        
//            using (var repo = new Repository(repoPath))                
//            {
//                // Check out the specified branch            
//                Branch branch = Commands.Checkout(repo, branchName);
//                var commits = branch.Commits.OrderBy(c => c.Author.When.LocalDateTime).ToList();

//                int commCnt = 0, commSkipped = 0;
//                Commit lastCommit = null; 
//                foreach (Commit commit in commits)
//                {
//                    commCnt++;
//                    if (lastCommit != null && commit.Author.When.LocalDateTime.Day != lastCommit.Author.When.LocalDateTime.Day)
//                    {
//                        //Console.WriteLine("Day change from {0} to {1}", lastCommit.Author.When.LocalDateTime.Date, commit.Author.When.LocalDateTime.Date);
//                        CountLines(repo, lastCommit, commCnt, commits.Count, commSkipped); // last commit of the day
//                    }
//                    else
//                    {
//                        commSkipped++;
//                    }
//                    lastCommit = commit;
//                }
//                if( lastCommit != null)
//                {
//                    CountLines(repo, lastCommit, commCnt, commits.Count, commSkipped);  // last commit of the day
//                }
//            }
//            return _stats;
//        }

//        void CountLines(Repository repo, Commit commit, int commCnt, int commTotal, int commSkipped)
//        {
//            int lineCount = CountLinesInTree(repo, commit.Tree);
//            _stats.Add(
//                commit.Author.When.LocalDateTime, 
//                commit.Sha, 
//                commit.Author.Email, 
//                lineCount, 
//                commCnt, 
//                commTotal, 
//                commSkipped);
//        }

//        static int CountLinesInTree(Repository repo, Tree tree)
//        {
//            int lineCount = 0;
//            var allowedExtensions = new HashSet<string> { ".cs", ".php", ".js", ".vue" }; // Add the file extensions you want to include

//            foreach (var entry in tree)
//            {                
//                if (entry.TargetType == TreeEntryTargetType.Blob)
//                {
//                    string extension = Path.GetExtension(entry.Name);
//                    if (!allowedExtensions.Contains(extension))
//                        continue;
//                    var blob = (Blob)entry.Target;
//                    using (var contentStream = blob.GetContentStream())
//                    using (var reader = new StreamReader(contentStream))
//                    {
//                        while (reader.ReadLine() != null)
//                        {
//                            lineCount++;
//                        }
//                    }
//                }
//                else if (entry.TargetType == TreeEntryTargetType.Tree)
//                {
//                    lineCount += CountLinesInTree(repo, (Tree)entry.Target);
//                }
//            }
            
//            return lineCount;
//        }
//    }
//}
