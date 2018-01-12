using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitFameSharp
{
    public class AuthorStatistics
    {
        public string Author { get; }
        public int TotalLineCount { get; private set; }
        public Dictionary<string, int> LineCountByFileExtension { get; } = new Dictionary<string, int>();
        public List<string> FilesContributedTo { get; } = new List<string>();
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
            FilesContributedTo.Add(file);
        }

        public override string ToString()
        {
            return Author;
        }

        public static AuthorStatistics MergeFrom(string newAuthorName, List<AuthorStatistics> existingAuthorStatistics)
        {
            var newAuthorStatistics = new AuthorStatistics(newAuthorName);
            newAuthorStatistics.CommitCount = existingAuthorStatistics.Sum(x => x.CommitCount);
            newAuthorStatistics.FilesContributedTo.AddRange(existingAuthorStatistics.SelectMany(x => x.FilesContributedTo).Distinct().ToList());
            newAuthorStatistics.TotalLineCount = existingAuthorStatistics.Sum(x => x.TotalLineCount);

            foreach (var stats in existingAuthorStatistics)
            {
                foreach (var extension in stats.LineCountByFileExtension.Keys)
                {
                    newAuthorStatistics.LineCountByFileExtension.TryGetValue(extension, out var count);
                    newAuthorStatistics.LineCountByFileExtension[extension] = count + stats.LineCountByFileExtension[extension];
                }
            }

            return newAuthorStatistics;
        }
    }
}