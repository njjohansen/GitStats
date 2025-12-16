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
    class CountStats : GitStatistics
    {
        private static int _totalLines = 0;
        private Dictionary<string, int> FileStats { get; set; } = new Dictionary<string, int>();
        public CountStats(string repoName, bool verbose = false): base(repoName, verbose)
        {            
        }

        public CountStats(string repoName): base(repoName, false)
        {

        }
        
       public void Add(string extension, int cnt)
        { 
            _totalLines += cnt;
            if (FileStats.TryGetValue(extension, out int oldCnt))
            {
                FileStats[extension] = oldCnt + cnt;
            }
            else
            {
                FileStats.Add(extension, cnt);
            }
        }

        public override void PrintSystem()
        {
            var table = CreatePrintTable();
            foreach (var fileStat in FileStats)
            {
                table.AddRow(fileStat.Key, fileStat.Value.ToString("N0", CultureInfo.InvariantCulture));
            }
            table.WriteSystem();
            Console.WriteLine("Repo: {0} analyzed in {1} bringing total count to: {2}", RepoName, Duration, _totalLines);
        }

        public override void PrintFriendly()
        {   
            var table = CreatePrintTable();
            foreach (var fileStat in FileStats)
            {
                table.AddRow(fileStat.Key, fileStat.Value.ToString("N0", CultureInfo.InvariantCulture));                
            }
            table.Write();
            Console.WriteLine("Repo: {0} analyzed in {1} bringing total count to: {2}", RepoName, Duration, _totalLines);            
        }

        protected override void Add(GitStatistics other)
        {            
            var otherCasted = other as CountStats;
            if (otherCasted == null)
                throw new InvalidCastException("Can only add CountStats to CountStats!");

            foreach( var count in otherCasted.FileStats)
            {
                this.Add(count.Key, count.Value);
            }

        }

        protected override StatsTable CreatePrintTable()
        {
            return new StatsTable("File Extension", "Line Count" );
        }
    }
}
