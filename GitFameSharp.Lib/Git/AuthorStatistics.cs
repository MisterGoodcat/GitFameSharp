using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitFameSharp.Git
{
    public sealed class AuthorStatistics
    {
        public string Author { get; }
        public int TotalLineCount { get; private set; }
        public Dictionary<string, int> LineCountByFileExtension { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> LineCountByFile { get; } = new Dictionary<string, int>();
        public int CommitCount { get; set; }

        public AuthorStatistics(string author)
        {
            Author = author;
        }

        public void ProcessFileStats(string file, int lineCount)
        {
            TotalLineCount += lineCount;
            var extension = Path.GetExtension(file)?.ToLower() ?? "[none]";
            LineCountByFileExtension.TryGetValue(extension, out var count);
            LineCountByFileExtension[extension] = count + lineCount;
            LineCountByFile[file] = lineCount;
        }

        public override string ToString()
        {
            return Author;
        }

        public static AuthorStatistics MergeFrom(string newAuthorName, List<AuthorStatistics> existingAuthorStatistics)
        {
            var newAuthorStatistics = new AuthorStatistics(newAuthorName);
            newAuthorStatistics.CommitCount = existingAuthorStatistics.Sum(x => x.CommitCount);
            newAuthorStatistics.TotalLineCount = existingAuthorStatistics.Sum(x => x.TotalLineCount);

            foreach (var stats in existingAuthorStatistics)
            {
                foreach (var extensionEntry in stats.LineCountByFileExtension)
                {
                    newAuthorStatistics.LineCountByFileExtension.TryGetValue(extensionEntry.Key, out var count);
                    newAuthorStatistics.LineCountByFileExtension[extensionEntry.Key] = count + extensionEntry.Value;
                }

                foreach (var fileEntry in stats.LineCountByFile)
                {
                    newAuthorStatistics.LineCountByFile.TryGetValue(fileEntry.Key, out var count);
                    newAuthorStatistics.LineCountByFile[fileEntry.Key] = count + fileEntry.Value;
                }
            }

            return newAuthorStatistics;
        }
    }
}