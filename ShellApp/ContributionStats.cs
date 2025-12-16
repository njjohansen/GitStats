using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellApp
{
    class ContributionStats : GitStatistics
    {
        protected Dictionary<string, string> _nameMap = new Dictionary<string, string>();
        protected Dictionary<string, AuthorStats> _authorStats = new Dictionary<string, AuthorStats>();

        public ContributionStats(string repoName, IEnumerable<Tuple<string, string>> nameMap, bool verbose = false) : base(repoName, verbose)
        {
            if (nameMap != null)
            {
                foreach (var pair in nameMap)
                {
                    _nameMap.Add(pair.Item1, pair.Item2);
                }
            }
        }
        public ContributionStats() : base("empty", false)
        {

        }

        protected string getAuthorId(string email)
        {
            string convertedName;
            if (_nameMap != null && _nameMap.TryGetValue(email, out convertedName))
                return convertedName;
            return email;
        }

        protected AuthorStats getAuthor(string name, string email)
        {
            string convertedId = getAuthorId(email);

            AuthorStats authorStats = null;
            if (!_authorStats.TryGetValue(convertedId, out authorStats))
            {
                authorStats = new AuthorStats(name, email);
                _authorStats.Add(convertedId, authorStats);
            }
            return authorStats;
        }

        public void RegisterSurvival(string name, string email, long lines)
        {
            AuthorStats authorStat = getAuthor(name, email);
            authorStat.RegisterSurvival(lines);
        }

        public void RegisterModification(string name, string email, DateTime when, long linesAdded, long linesRemoved)
        {
            AuthorStats authorStat = getAuthor(name, email);
            authorStat.RegisterModification(when, linesAdded, linesRemoved);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (AuthorStats authorStat in _authorStats.Values)
            {
                sb.AppendLine(authorStat.ToString());
            }
            return sb.ToString();
        }

        protected override void Add(GitStatistics other)
        {
            var otherCasted = other as ContributionStats;
            if (otherCasted == null)
                throw new InvalidCastException("Can only add CountStats to CountStats!");
            //TODO
        }

        public override void PrintSystem()
        {
            Console.WriteLine(ToString());
        }

        public override void PrintFriendly()
        {
            Console.WriteLine(ToString());
        }
    }

    class AuthorStats
    {
        public AuthorStats(string name, string email)
        {
            Name = name;
            Email = email;
            LinesRemoved = 0;
            LinesAdded = 0;
            LinesSurvived = 0;
        }
        public string Name { get; private set; }
        public string Email { get; private set; }

        protected List<Modification> modifications = new List<Modification>();
        public List<Modification> Modifications { get { return modifications; } }

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
                foreach (Modification mod in modifications)
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
            Modifications.Add(new Modification(when, linesAdded, linesRemoved));
            _sorted = false;
        }

        public void RegisterSurvival(long lines)
        {
            LinesSurvived += lines;
        }

        public override string ToString()
        {
            return String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", Email, LinesAdded, LinesRemoved, LinesSurvived, FirstCommit.ToString("yyyy-MM-dd"), LastCommit.ToString("yyyy-MM-dd"), DaysOfCommits, PeriodOfCommits.TotalDays.ToString("F2"), NumberOfCommits);
        }
    }

    public class Modification
    {
        public Modification(DateTime when, long linesAdded, long linesRemoved)
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
