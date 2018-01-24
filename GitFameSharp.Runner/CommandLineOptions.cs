using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GitFameSharp.AuthorMerge;
using GitFameSharp.Git;

namespace GitFameSharp.Runner
{
    public class CommandLineOptions
    {
        public static void PrintVersion(Action<string> writeMessage)
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            writeMessage("GitFameSharp version: " + version);
        }

        public static void PrintUsage(Action<string> writeMessage)
        {
            PrintVersion(writeMessage);
            writeMessage("Usage: ");
            writeMessage(string.Empty);
            writeMessage("--Help");
            PrintSplitMessage(writeMessage, "Prints this help screen and exits.");
            writeMessage("--Version");
            PrintSplitMessage(writeMessage, "Prints the version and exits.");
            writeMessage($"--{nameof(GitDir)}=\"[path]\"");
            PrintSplitMessage(writeMessage, $"The path to the Git directory to analyze. Default: \"{DefaultGitDir}\"");
            writeMessage($"--{nameof(Branch)}=\"[branch]\"");
            PrintSplitMessage(writeMessage, $"The branch to analyze. Default: \"{DefaultBranch}\"");
            writeMessage($"--{nameof(Exclude)}=\"[RegEx]\"");
            PrintSplitMessage(writeMessage, "A regular expression (.NET flavor) to determine which files or folders to exclude. Default: [empty]");
            writeMessage($"--{nameof(Include)}=\"[RegEx]\"");
            PrintSplitMessage(writeMessage, "A regular expression (.NET flavor) to determine which files to include. Only inspects the files that have not been excluded by the --Exclude option. Default: [empty]");
            writeMessage($"--{nameof(ParallelBlameProcesses)}=[number]");
            PrintSplitMessage(writeMessage, $"The number of CPU cores to use in parallel. Default: [Number of cores on your machine: {DefaultParallelBlameProcesses}]");
            writeMessage($"--{nameof(Output)}=\"[path]\"");
            PrintSplitMessage(writeMessage, $"The target file the results should be written to in CSV format. Leave empty to prevent output to file. Default: \"{DefaultOutput}\"");
            writeMessage($"--{nameof(VerboseOutput)}=\"true|false\"");
            PrintSplitMessage(writeMessage, $"When writing an output CSV file, determines whether one line is written for each file an author contributed to. \"false\" only outputs aggregated summary lines per file extension. Default: \"{DefaultOutputVerbose}\"");
            writeMessage($"--{nameof(AuthorsToMerge)}=\"[list of aliases]\"");
            PrintSplitMessage(writeMessage, "Multiple author aliases to be merged into a single statistic. Syntax: Put each group of aliases into brackets, use the pipe symbol to separate aliases. E.g: \"[Author A alias 1|Author A alias 2][Author B alias 1|Author B alias 2]\". The first alias entry is used as the author name of the aggregated result. You can use a non-existing author alias as the first entry to beautify the author name. Default: [empty]");
        }

        private static void PrintSplitMessage(Action<string> writeMessage, string message)
        {
            const int indent = 4;
            const int maxLineContentLength = 76;

            while (!string.IsNullOrWhiteSpace(message))
            {
                var maxLength = maxLineContentLength > message.Length ? message.Length : maxLineContentLength;
                var nextSpaceIndex = message.LastIndexOf(' ', maxLength - 1);
                if (nextSpaceIndex == -1 || maxLength < maxLineContentLength)
                {
                    nextSpaceIndex = maxLength;
                }

                writeMessage($"{new string(' ', indent)}{message.Substring(0, nextSpaceIndex)}");
                message = message.Substring(nextSpaceIndex).Trim();
            }

            writeMessage(string.Empty);
        }

        public const string DefaultGitDir = ".";
        public const string DefaultBranch = "HEAD";
        public const string DefaultOutput = "result.csv";
        public const bool DefaultOutputVerbose = false;
        public static readonly int DefaultParallelBlameProcesses = Environment.ProcessorCount;

        public string GitDir { get; set; } = DefaultGitDir;
        public string Branch { get; set; } = DefaultBranch;
        public string Include { get; set; }
        public string Exclude { get; set; }
        public int ParallelBlameProcesses { get; set; } = DefaultParallelBlameProcesses;
        public string Output { get; set; } = DefaultOutput;
        public bool VerboseOutput { get; set; }
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

            var mergeGroups = AuthorsToMerge.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('[', ']'));
            foreach (var mergeGroup in mergeGroups)
            {
                var authorsToMerge = mergeGroup.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                if (authorsToMerge.Count == 0)
                {
                    continue;
                }

                lookup[authorsToMerge[0]] = authorsToMerge.ToList();
            }

            return new AuthorMergeOptions(lookup);
        }

        public bool Validate(Action<string> writeMessage)
        {
            var result = true;

            if (string.IsNullOrWhiteSpace(GitDir))
            {
                writeMessage?.Invoke("ERROR: No git directory provided.");
                result = false;
            }

            if (string.IsNullOrWhiteSpace(Branch))
            {
                writeMessage?.Invoke("ERROR: No branch provided.");
                result = false;
            }

            if (ParallelBlameProcesses < 1)
            {
                writeMessage?.Invoke("ERROR: The number of parallel blame processes must be greater than 0.");
                result = false;
            }

            if (!result)
            {
                writeMessage?.Invoke("Use --Help to display a usage screen.");
            }

            return result;
        }
    }
}