using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellApp
{
    public abstract class GitAnalysis
    {
        public GitAnalysis(bool verbose)
        {
            Verbose = verbose;            
        }
        protected bool Verbose { get; set; }
        public abstract GitStatistics Analyze(string repoName, DateTime startTime, AppSettings settings);
    }
}
