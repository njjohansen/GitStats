using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShellApp
{
    class EmptyStats : GitStatistics
    {        
        public EmptyStats(string repoName, bool verbose = false): base(repoName, verbose)
        {
            
        }

        public EmptyStats() : base("Empty", false)
        {

        }

        public override void PrintSystem()
        {
            Console.WriteLine("{0}", RepoName);
        }

        public override void PrintFriendly()
        {
            Console.WriteLine("Repo: {0} analyzed in {1}", RepoName, Duration);
        }

        protected override void Add(GitStatistics other)
        {
            
        }
    }
}
