//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace ShellApp
//{
//    class HistoryStats : GitStatistics
//    {
//        private bool Verbose { get; set; }
//        private List<DateStats> DateStats { get; set; }
//        public HistoryStats(bool verbose)
//        {
//            Verbose = verbose;
//            DateStats = new List<DateStats>();
//        }
//        public
//       void Add(DateTime date, string sha, string email, int lineCount, int commCnt, int commTotal, int commSkipped)
//        {
//            if (Verbose)
//                Console.WriteLine("({4}%) [{0}] - After {1} by {2}, {3} lines. ({5} skipped)", date.ToString("yyyy-MM-dd"), sha, email, lineCount, (100 * (double)commCnt / commTotal).ToString("0.0"), commSkipped);
//            DateStats.Add(new DateStats(date, sha, lineCount));
//        }

//        public void PrintSystem()
//        {
//            foreach (var dateStat in DateStats)
//            {
//                Console.WriteLine("{0};{1};{2}", dateStat.Date.ToString("yyyy-MM-dd"), dateStat.Sha, dateStat.LineCount);
//            }
//        }

//        public void PrintFriendly()
//        {
//            foreach(var dateStat in DateStats)
//            {
//                Console.WriteLine("[{0}] After {1}: {2} lines.", dateStat.Date.ToString("yyyy-MM-dd"), dateStat.Sha, dateStat.LineCount);
//            }
//        }
//    }

//    class DateStats
//    {
//        public DateTime Date { get; }
//        public string Sha { get; }        
//        public int LineCount { get; }        

//        public DateStats(DateTime date, string sha, int lineCount)
//        {
//            Sha = sha;
//            LineCount = lineCount;
//            Date = date;
//        }
//    }
//}
