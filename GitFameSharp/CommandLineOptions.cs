using System;
using System.Collections.Generic;
using System.Linq;
using GitFameSharp.AuthorMerge;
using GitFameSharp.Git;

namespace GitFameSharp
{
    public class CommandLineOptions
    {
        public string GitDir { get; set; } = ".";
        public string Branch { get; set; } = "HEAD";
        public string Include { get; set; }
        public string Exclude { get; set; }
        public int ParallelBlameProcesses { get; set; } = Environment.ProcessorCount;
        public string Output { get; set; } = "result.csv";
        public string AuthorsToMerge { get; set; }

        public GitOptions GetGitOptions()
        {
            return new GitOptions
            {
                Branch = Branch,
                Exclude = Exclude,
                GitDir = GitDir,
                Include = Include
            };
        }

        public GitFileAnalyzerOptions GetFileAnalyzerOptions()
        {
            return new GitFileAnalyzerOptions
            {
                ParallelBlameProcesses = ParallelBlameProcesses
            };
        }

        public AuthorMergeOptions GetAuthorMergeOptions()
        {
            var lookup = new Dictionary<string, ICollection<string>>();

            if (string.IsNullOrWhiteSpace(AuthorsToMerge))
            {
                return new AuthorMergeOptions(lookup);
            }

            var mergeGroups = AuthorsToMerge.Split("][", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('[', ']'));
            foreach (var mergeGroup in mergeGroups)
            {
                var authorsToMerge = mergeGroup.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                if (authorsToMerge.Count == 0)
                {
                    continue;
                }

                lookup[authorsToMerge[0]] = authorsToMerge.ToList();
            }

            return new AuthorMergeOptions(lookup);
        }
    }
}