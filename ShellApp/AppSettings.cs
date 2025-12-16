using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellApp
{
    public class AppSettings
    {

        public GitSettings Git { get; set; }
        public StatisticsSettings Statistics { get; set; }
        public AppSettings()
        {
        }
    }

    public class GitSettings
    {        
        public string[] Repositories { get; set; }

        private string workingDirectory;        
        public string WorkingDirectory 
        { 
            get => String.IsNullOrEmpty(workingDirectory)? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitStatistics"): workingDirectory; 
            set => workingDirectory = value; 
        }
        public string PersonalAccessToken { get; set; }
        public string ProxyServer { get; set; }
        private string branch;
        public string Branch { 
            get => String.IsNullOrEmpty(branch) ? "master" : branch; 
            set => branch = value; 
        }
    }

    public class StatisticsSettings
    {        
        public string[] Extensions { get; set; }

        private string[]? _gitExtensionFilter;
        /// <summary>
        /// Adds a '*' before each extension for git filtering, e.g. ".cs" becomes "*.cs".
        /// </summary>
        public string[] GitExtensionFilter
        {
            get
            {
                if (_gitExtensionFilter == null)
                {
                    return _gitExtensionFilter = Extensions.Select(e => "*" + e).ToArray() ;
                }
                return _gitExtensionFilter;
            }
        }
        public int CodeLineMinLength { get; set; }
        public bool IgnoreComments { get; set; }


        private HashSet<string>? _extensionsSet;
        /// <summary>
        ///  Creates a HashSet from Extensions for faster lookup.
        /// </summary>
        public HashSet<string> ExtensionsSet {
            get 
            {
                if(_extensionsSet == null)
                {
                    _extensionsSet = new HashSet<string>(Extensions);
                }
                return _extensionsSet;
            }  
        }
    }
}
