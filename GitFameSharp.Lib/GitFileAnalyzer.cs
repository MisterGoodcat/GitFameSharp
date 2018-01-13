using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitFameSharp.Git;

namespace GitFameSharp
{
    public class GitFileAnalyzer
    {
        private readonly GitFileAnalyzerOptions _options;
        private readonly GitCommands _git;

        public GitFileAnalyzer(GitFileAnalyzerOptions options, GitCommands git)
        {
            _options = options;
            _git = git;
        }

        public async Task<ICollection<AuthorStatistics>> BlameFilesAsync(ICollection<string> files, Action<Progress> progressReporter = null)
        {
            var result = new Dictionary<string, AuthorStatistics>();

            var sw = new Stopwatch();
            sw.Start();

            var fileCounter = 0;

            await files.ForEachAsync(_options.ParallelBlameProcesses, async file =>
            {
                var fileStatistics = await _git.BlameAsync(file).ConfigureAwait(false);

                lock (result)
                {
                    // process results
                    foreach (var author in fileStatistics.Authors)
                    {
                        if (!result.TryGetValue(author, out var stats))
                        {
                            stats = new AuthorStatistics(author);
                            result[author] = stats;
                        }

                        stats.ProcessFileStats(file, fileStatistics.LinesCountByAuthor[author]);
                    }

                    if (progressReporter != null)
                    {
                        fileCounter++;
                        var progress = new Progress(fileCounter, files.Count, sw.Elapsed);
                        progressReporter(progress);
                    }
                }
            }).ConfigureAwait(false);

            sw.Stop();
            return result.Values.ToList();
        }

        public sealed class Progress
        {
            public int FinishedFiles { get; }
            public int TotalFiles { get; }
            public double ElapsedSeconds { get; }
            public double EstimatedTotalSeconds { get; }
            public TimeSpan RemainingTime { get; }

            internal Progress(int finishedFiles, int totalFiles, TimeSpan elapsedTime)
            {
                FinishedFiles = finishedFiles;
                TotalFiles = totalFiles;
                ElapsedSeconds = elapsedTime.TotalSeconds;
                EstimatedTotalSeconds = (totalFiles / (double)finishedFiles) * ElapsedSeconds;
                RemainingTime = TimeSpan.FromSeconds(EstimatedTotalSeconds - ElapsedSeconds);
            }
        }
    }
}