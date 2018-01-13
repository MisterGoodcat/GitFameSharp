using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitFameSharp.Git;

namespace GitFameSharp.Output
{
    public sealed class CsvExporter
    {
        public string ConvertToCsv(ICollection<AuthorStatistics> authorStatistics)
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
    }
}