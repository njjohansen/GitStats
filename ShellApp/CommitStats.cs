using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShellApp
{
    class CommitStats : GitStatistics
    {
        protected Dictionary<string, string> _nameMap = new Dictionary<string, string>();
        protected Dictionary<string, CommitAuthorStats> _authorStats = new Dictionary<string, CommitAuthorStats>();
        
        public CommitStats(string repoName, IEnumerable<Tuple<string, string>> nameMap, bool verbose = false) : base(repoName, verbose)
        {
            if (nameMap != null)
            {
                foreach (var pair in nameMap)
                {
                    _nameMap.Add(pair.Item1, pair.Item2);
                }
            }
        }
        protected CommitStats() : base("empty", false)
        {

        }

        public CommitStats(string repoName) : base(repoName, false)
        {

        }

        protected string getAuthorId(string email)
        {
            string convertedName;
            if (_nameMap != null && _nameMap.TryGetValue(email, out convertedName))
                return convertedName;
            return email;
        }

        protected CommitAuthorStats getAuthor(string name, string email)
        {
            string convertedId = getAuthorId(email);

            CommitAuthorStats authorStats = null;
            if (!_authorStats.TryGetValue(convertedId, out authorStats))
            {
                authorStats = new CommitAuthorStats(name, email);
                _authorStats.Add(convertedId, authorStats);
            }
            return authorStats;
        }

        public void RegisterSurvival(string name, string email, long lines)
        {
            CommitAuthorStats authorStat = getAuthor(name, email);
            authorStat.RegisterSurvival(lines);
        }

        public void RegisterModification(string name, string email, DateTime when, long linesAdded, long linesRemoved)
        {
            CommitAuthorStats authorStat = getAuthor(name, email);
            authorStat.RegisterModification(when, linesAdded, linesRemoved);
        }
   
        protected override void Add(GitStatistics other)
        {
            var otherCasted = other as CommitStats;
            if (otherCasted == null)
                throw new InvalidCastException("Can only add CountStats to CountStats!");

            // Merge name maps (do not overwrite existing mappings)
            if (otherCasted._nameMap != null)
            {
                foreach (var kvp in otherCasted._nameMap)
                {
                    if (!_nameMap.ContainsKey(kvp.Key))
                        _nameMap[kvp.Key] = kvp.Value;
                }
            }

            // Merge author stats
            if (otherCasted._authorStats != null)
            {
                foreach (var kvp in otherCasted._authorStats)
                {
                    var id = kvp.Key;
                    var otherAuthor = kvp.Value;

                    if (_authorStats.TryGetValue(id, out var existing))
                    {
                        // accumulate numeric totals
                        existing.LinesAdded += otherAuthor.LinesAdded;
                        existing.LinesRemoved += otherAuthor.LinesRemoved;
                        existing.LinesSurvived += otherAuthor.LinesSurvived;

                        // merge modifications
                        existing.Modifications.AddRange(otherAuthor.Modifications);
                        existing._sorted = false;
                    }
                    else
                    {
                        // clone otherAuthor into a new CommitAuthorStats
                        var clone = new CommitAuthorStats(otherAuthor.Name, otherAuthor.Email);
                        clone.LinesAdded = otherAuthor.LinesAdded;
                        clone.LinesRemoved = otherAuthor.LinesRemoved;
                        clone.LinesSurvived = otherAuthor.LinesSurvived;
                        clone.Modifications.AddRange(otherAuthor.Modifications);
                        clone._sorted = otherAuthor._sorted;
                        _authorStats.Add(id, clone);
                    }
                }
            }
        }

        public override void PrintSystem()
        {
            Console.WriteLine($"Repo: {RepoName}");
            var table = CreatePrintTable();            
            foreach (CommitAuthorStats authorStat in _authorStats.Values)
            {
                authorStat.AddRow(table);
            }
            table.WriteSystem();
        }

        public override void PrintFriendly()
        {
            Console.WriteLine($"Repo: {RepoName}");
            var table = CreatePrintTable();
            foreach (CommitAuthorStats authorStat in _authorStats.Values)
            {
                authorStat.AddRow(table);
            }
            table.Write(ConsoleTables.Format.Minimal);
        }

        protected override StatsTable CreatePrintTable()
        {
            return new StatsTable(
                "Email",
                "Added",
                "Removed",
                "Survived",
                "1st Commit",
                "Last Commit",
                "Commit days",
                "Commits period",
                "Commit Cnt"
             );
        }
    }

    class CommitAuthorStats
    {
        public CommitAuthorStats(string name, string email)
        {
            Name = name;
            Email = email;
            LinesRemoved = 0;
            LinesAdded = 0;
            LinesSurvived = 0;
        }
        public string Name { get; private set; }
        public string Email { get; private set; }

        protected List<CommitModification> modifications = new List<CommitModification>();
        public List<CommitModification> Modifications { get { return modifications; } }

        public long LinesAdded { get; set; }
        public long LinesRemoved { get; set; }
        public long LinesSurvived { get; set; }

        public bool _sorted = false;

        private void EnsureModificationSort()
        {
            if (!_sorted)
            {
                modifications.Sort((m1, m2) => m1.When.CompareTo(m2.When));
                _sorted = true;
            }
        }

        public long DaysOfCommits
        {
            get
            {
                EnsureModificationSort();
                long cnt = 0;
                DateTime last = DateTime.MinValue;
                foreach (CommitModification mod in modifications)
                {
                    if (mod.When.Date != last)
                    {
                        cnt++;
                        last = mod.When.Date;
                    }
                }
                return cnt;
            }
        }

        public long NumberOfCommits => modifications.Count;
        public TimeSpan PeriodOfCommits => LastCommit - FirstCommit;

        public DateTime FirstCommit
        {
            get
            {
                EnsureModificationSort();
                if (modifications.Count > 0)
                    return modifications[0].When;
                return DateTime.MinValue;
            }
        }

        public DateTime LastCommit
        {
            get
            {
                EnsureModificationSort();
                if (modifications.Count > 0)
                    return modifications[modifications.Count - 1].When;
                return DateTime.MinValue;
            }
        }

        public void RegisterModification(DateTime when, long linesAdded, long linesRemoved)
        {
            LinesAdded += linesAdded;
            LinesRemoved += linesRemoved;
            Modifications.Add(new CommitModification(when, linesAdded, linesRemoved));
            _sorted = false;
        }

        public void RegisterSurvival(long lines)
        {
            LinesSurvived += lines;
        }

        public void AddRow(StatsTable table)
        {
            table.AddRow(GitStatistics.TruncateStr(Email, 30), LinesAdded, LinesRemoved, LinesSurvived, FirstCommit.ToString("yyyy-MM-dd"), LastCommit.ToString("yyyy-MM-dd"), DaysOfCommits, PeriodOfCommits.TotalDays.ToString("F2"), NumberOfCommits);
        }

        public override string ToString()
        {
            return String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", Email, LinesAdded, LinesRemoved, LinesSurvived, FirstCommit.ToString("yyyy-MM-dd"), LastCommit.ToString("yyyy-MM-dd"), DaysOfCommits, PeriodOfCommits.TotalDays.ToString("F2"), NumberOfCommits);
        }
    }

    public class CommitModification
    {
        public CommitModification(DateTime when, long linesAdded, long linesRemoved)
        {
            When = when;
            LinesAdded = linesAdded;
            LinesRemoved = linesRemoved;
        }
        public DateTime When { get; set; }
        public long LinesAdded { get; set; }
        public long LinesRemoved { get; set; }
    }
}
