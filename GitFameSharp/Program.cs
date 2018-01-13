using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitFameSharp.AuthorMerge;
using GitFameSharp.Git;
using GitFameSharp.Output;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;

namespace GitFameSharp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var options = InitializeOptions(args);
            var git = new GitCommands(options.GetGitOptions());
            var files = await git.GetFilesAsync().ConfigureAwait(false);

            var gitFileAnalyzer = new GitFileAnalyzer(options.GetFileAnalyzerOptions(), git);

            ICollection<AuthorStatistics> authorStatistics = null;

            using (var progressBar = CreateProgressBar(files.Count))
            {
                // ReSharper disable once AccessToDisposedClosure => false positive
                authorStatistics = await gitFileAnalyzer.BlameFilesAsync(files, progress => AdvanceProgressBar(progressBar, progress)).ConfigureAwait(false);

                var commitStatistics = await git.ShortlogAsync().ConfigureAwait(false);
                foreach (var commitStatistic in commitStatistics)
                {
                    var authorStatistic = authorStatistics.SingleOrDefault(x => x.Author.Equals(commitStatistic.Key));
                    if (authorStatistic == null)
                    {
                        authorStatistic = new AuthorStatistics(commitStatistic.Key);
                        authorStatistics.Add(authorStatistic);
                    }

                    authorStatistic.CommitCount = commitStatistic.Value;
                }

                var merger = new AuthorsMerger(options.GetAuthorMergeOptions());
                authorStatistics = merger.Merge(authorStatistics);
            }

            WriteOutput(options, authorStatistics);
            DisplaySummary(authorStatistics);
        }

        private static CommandLineOptions InitializeOptions(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var options = new CommandLineOptions();
            configuration.Bind(options);

            return options;
        }

        private static ProgressBar CreateProgressBar(int totalTickCount)
        {
            var options = new ProgressBarOptions
            {
                ProgressBarOnBottom = true,
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkYellow,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };

            return new ProgressBar(totalTickCount, "Starting...", options);
        }

        private static void AdvanceProgressBar(ProgressBar progressBar, GitFileAnalyzer.Progress progress)
        {
            progressBar.Tick($"Finished {progress.FinishedFiles} of {progress.TotalFiles} (Remaining: {progress.RemainingTime.TotalHours:00}:{progress.RemainingTime:mm\\:ss})");
        }

        private static void WriteOutput(CommandLineOptions commandLineOptions, ICollection<AuthorStatistics> authorStatistics)
        {
            var outputPath = commandLineOptions.Output;
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return;
            }

            foreach (var invalidPathChar in Path.GetInvalidPathChars())
            {
                outputPath = outputPath.Replace(invalidPathChar, '_');
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var exporter = new CsvExporter();
            var content = exporter.ConvertToCsv(authorStatistics);

            File.WriteAllText(outputPath, content, Encoding.UTF8);
        }

        private static void DisplaySummary(ICollection<AuthorStatistics> authorStatistics)
        {
            var header = $"|{"Author".PadRight(30)}|{"LOC".PadLeft(10)}|{"Commits".PadLeft(10)}|{"Files".PadLeft(10)}|";
            var separator = $"+{new string('-', header.Length - 2)}+";
            Console.WriteLine(separator);
            Console.WriteLine(header);
            Console.WriteLine(separator);

            foreach (var author in authorStatistics.OrderByDescending(x => x.TotalLineCount))
            {
                Console.WriteLine($"|{author.Author.PadRight(30)}|{author.TotalLineCount.ToString().PadLeft(10)}|{author.CommitCount.ToString().PadLeft(10)}|{author.FilesContributedTo.Count.ToString().PadLeft(10)}|");
            }

            Console.WriteLine(separator);
            Console.ReadLine();
        }
    }
}