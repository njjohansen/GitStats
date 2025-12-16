using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTables;

namespace ShellApp
{
    public class StatsTable: ConsoleTable
    {
        public StatsTable(params string[] columns) : base(columns)
        {
        }
        public void WriteSystem()
        {
            Console.WriteLine(string.Join(";", Columns));
            foreach (var row in Rows) 
            {
                Console.WriteLine(string.Join(";", row)); 
            }
        }
    }
}
