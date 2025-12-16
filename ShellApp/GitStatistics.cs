using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShellApp
{
    public abstract class GitStatistics
    {
        protected GitStatistics(string repoName, bool verbose = false)
        {
            Verbose = verbose;
            RepoName = repoName;
        }
        private Stopwatch _sw = new Stopwatch();
        private TimeSpan _ts;
        public static GitStatistics operator +(GitStatistics a, GitStatistics b)
        {
            a.Add(b);
            a._ts += b._ts;
            return a;
        }        

        protected abstract void Add(GitStatistics other);

        protected bool Verbose { get; set; }
        public string RepoName { get; set; }
        public abstract void PrintSystem();
        public abstract void PrintFriendly();
        public void Start() 
        {
            _sw.Start();
        }
        public void Stop()
        {
            _sw.Stop();
            _ts = _sw.Elapsed;
        }

        protected string Duration
        {
            get { return _ts.ToString(@"hh\:mm\:ss\.fff"); }
        }

    }

    public class GitStatisticsList<T>: IEnumerable<T> where T : GitStatistics, new()
    {
        List<T> _list = new List<T>();
        public GitStatisticsList()
        {

        }

        public void Add(T stats)
        {
            _list.Add(stats);
        }

        public T Consolidate()
        {
            if (_list == null || _list.Count == 0)
                throw new ArgumentException("The list cannot be null or empty.");

            // Start with the first item as the seed
            T result = new T();
            result.RepoName = "Consolidated";

            // Iterate through the remaining items and add them to the result
            for (int i = 0; i < _list.Count; i++)
            {
                result = (T) (result + _list[i]);
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
