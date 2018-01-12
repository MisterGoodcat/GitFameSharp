using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ShellProgressBar;

namespace GitFameSharp
{
    public class GitFileAnalyzer
    {
        private readonly Options _options;
        private readonly Git _git;

        public GitFileAnalyzer(Options options, Git git)
        {
            _options = options;
            _git = git;
        }

        public async Task<ICollection<AuthorStatistics>> BlameFilesAsync(ICollection<string> files)
        {
            var result = new Dictionary<string, AuthorStatistics>();

            var sw = new Stopwatch();
            sw.Start();
            using (var progressBar = CreateProgressBar(files))
            {
                await files.ForEachAsync(_options.ParallelBlameProcesses, async file =>
                {
                    var fileStatistics = await _git.GetFileStatisticsAsync(file).ConfigureAwait(false);

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

                        // ReSharper disable once AccessToDisposedClosure => false positive, this whole call is awaited
                        AdvanceProgressBar(progressBar, sw, files);
                    }
                }).ConfigureAwait(false);
            }

            sw.Stop();
            return result.Values.ToList();
        }
        
        private static ProgressBar CreateProgressBar(ICollection<string> files)
        {
            var options = new ProgressBarOptions
            {
                ProgressBarOnBottom = true,
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkYellow,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };
            return new ProgressBar(files.Count, "Starting...", options);
        }

        private static void AdvanceProgressBar(ProgressBar progressBar, Stopwatch sw, ICollection<string> files)
        {
            var finishedFiles = progressBar.CurrentTick + 1;
            var elapsedSeconds = sw.Elapsed.TotalSeconds;
            var totalSeconds = (files.Count / (double)finishedFiles) * elapsedSeconds;
            var remaining = TimeSpan.FromSeconds(totalSeconds - elapsedSeconds);

            progressBar.Tick($"File {finishedFiles} of {files.Count} (Remaining: {remaining.TotalHours:00}:{remaining:mm\\:ss})");
        }
    }
}
