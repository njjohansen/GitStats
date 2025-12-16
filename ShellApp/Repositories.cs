using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShellApp
{
    static class Repositories
    {

  
        public static void CloneOrPullRepository(string repoUrl, string apiKey, string proxyUrl, string workingDirectory)
        {
            var repoName = new Uri(repoUrl).Segments.Last().TrimEnd('/');
            var repoUrlHost = new Uri(repoUrl).Host;
            var repoPath = Path.Combine(workingDirectory, repoName);
            var encodedPat = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey));

            Console.WriteLine($"Cloning repository '{repoName}' to '{repoPath}'...");

            if (Directory.Exists(repoPath))
            {
                var fetchOptions = new FetchOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
                    {
                        Username = $"",
                        Password = apiKey
                    },
                };
                //if (!String.IsNullOrEmpty(proxyUrl))
                //{
                //    fetchOptions.ProxyOptions.Url = proxyUrl;
                //    fetchOptions.ProxyOptions.ProxyType = ProxyType.Specified;
                //    CertificateCheckHandler certificateCheckHandler = (certificate, valid, host) =>
                //    {
                //        valid = true;
                //        return true;
                //    };
                //    fetchOptions.CertificateCheck = certificateCheckHandler;
                //}
                // Pull changes if the repository already exists
                using (var repo = new Repository(repoPath))
                {
                    var remote = repo.Network.Remotes["origin"];
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).ToList();
                    Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, null);

                    var mergeResult = Commands.Pull(repo, new Signature("Merger", "merger@example.com", DateTimeOffset.Now), new PullOptions{FetchOptions = fetchOptions});
                    Console.WriteLine($"Pulled changes for {repoName}: {mergeResult.Status}");
                }
            }
            else
            {
                // Clone the repository if it does not exist
                var co = new CloneOptions();
                //co.FetchOptions.CustomHeaders = new[] { $"Authorization: Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey))}" };
                co.FetchOptions.CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
                {
                    Username = $"",
                    Password = apiKey
                };
                //if (!String.IsNullOrEmpty(proxyUrl))
                //{
                //    co.FetchOptions.ProxyOptions.Url = proxyUrl;
                //    co.FetchOptions.ProxyOptions.ProxyType = ProxyType.Specified;
                //}
                Repository.Clone(repoUrl, repoPath, co);
                Console.WriteLine($"Cloned repository {repoName} to {repoPath}");
            }
        }


    }
}
