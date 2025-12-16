using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibGit2Sharp;

namespace ShellApp
{
    public class UpdateAnalysis : GitAnalysis
    {        
        private EmptyStats _stats;

        public UpdateAnalysis(bool verbose = false): base(verbose)
        {
            Verbose = verbose;            
        }
        public override GitStatistics Analyze(string repoName, DateTime startDate, AppSettings settings)
        {
            _stats = new EmptyStats(repoName, Verbose);
            _stats.Start();
            var repoPath = Path.Combine(settings.Git.WorkingDirectory, repoName);

            var repoUrl = getRepoUrl(repoName, settings);
            if(string.IsNullOrEmpty(repoUrl))
            {
                Console.WriteLine($"Repository '{repoName}' not found in settings");
                _stats.Stop();
                return _stats;
            }

            Repositories.CloneOrPullRepository(repoUrl, settings.Git.PersonalAccessToken, settings.Git.ProxyServer, settings.Git.WorkingDirectory);
            
            _stats.Stop();
            return _stats;
        }

        private string getRepoUrl(string repoName, AppSettings settings)
        {
            return settings.Git.Repositories.FirstOrDefault(r => new Uri(r).Segments.Last().TrimEnd('/') == repoName);
        }
    }
}
