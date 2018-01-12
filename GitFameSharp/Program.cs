using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GitFameSharp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var options = InitializeOptions(args);
            var git = new Git(options);
            var files = await git.GetFilesAsync().ConfigureAwait(false);

            var gitFileAnalyzer = new GitFileAnalyzer(options, git);
            var authorStatistics = await gitFileAnalyzer.BlameFilesAsync(files).ConfigureAwait(false);

            var commitStatistics = await git.GetCommitCountByAuthorAsync().ConfigureAwait(false);
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

            authorStatistics = MergeAuthors(options, authorStatistics);
            WriteOutput(options, authorStatistics);
            DisplaySummary(authorStatistics);
        }

        private static ICollection<AuthorStatistics> MergeAuthors(Options options, ICollection<AuthorStatistics> authorStatistics)
        {
            var processedAuthors = new List<string>();
            var result = new List<AuthorStatistics>();

            if (!string.IsNullOrWhiteSpace(options.AuthorsToMerge))
            {
                var mergeGroups = options.AuthorsToMerge.Split("][", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim('[', ']'));
                foreach (var mergeGroup in mergeGroups)
                {
                    var authorsToMerge = mergeGroup.Split('|', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    if (authorsToMerge.Count == 0)
                    {
                        continue;
                    }
                    
                    var existingAuthorStatistics = authorStatistics.Where(x => authorsToMerge.Contains(x.Author)).ToList();
                    var newName = authorsToMerge.First();
                    var newAuthorStatistics = AuthorStatistics.MergeFrom(newName, existingAuthorStatistics);
                    result.Add(newAuthorStatistics);

                    processedAuthors.AddRange(authorsToMerge);
                }
            }

            var unprocessedAuthors = authorStatistics.Where(x => !processedAuthors.Contains(x.Author)).ToList();
            return result.Concat(unprocessedAuthors).ToList();
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

        private static void WriteOutput(Options options, ICollection<AuthorStatistics> authorStatistics)
        {
            var outputPath = options.Output;
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

            var content = ConvertToCsv(authorStatistics);

            File.WriteAllText(outputPath, content, Encoding.UTF8);
        }

        private static string ConvertToCsv(ICollection<AuthorStatistics> authorStatistics)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Author;File Extension;Lines By File Extension;Total Commit Count;Total Files Contributed To");
            foreach (var authorStatistic in authorStatistics.OrderBy(x => x.Author))
            {
                foreach (var extension in authorStatistic.LineCountByFileExtension.Keys)
                {
                    sb.Append("\"");
                    sb.Append(authorStatistic.Author);
                    sb.Append("\";\"");
                    sb.Append(extension);
                    sb.Append("\";");
                    sb.Append(authorStatistic.LineCountByFileExtension[extension]);
                    sb.Append(";");
                    sb.Append(authorStatistic.CommitCount);
                    sb.Append(";");
                    sb.Append(authorStatistic.FilesContributedTo.Count);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static Options InitializeOptions(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var options = new Options();
            configuration.Bind(options);

            return options;
        }
    }
}