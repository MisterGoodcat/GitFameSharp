using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitFameSharp.Git;

namespace GitFameSharp.Output
{
    public sealed class CsvExporter
    {
        public string ConvertToCsv(ICollection<AuthorStatistics> authorStatistics, bool verbose)
        {
            return verbose ? 
                GenerateLinesByFile(authorStatistics) : 
                GenerateLinesByFileExtension(authorStatistics);
        }

        private string GenerateLinesByFile(ICollection<AuthorStatistics> authorStatistics)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Author;File;File Extension;Lines By File;Total Commit Count;Total Files Contributed To");
            foreach (var authorStatistic in authorStatistics.OrderBy(x => x.Author))
            {
                foreach (var file in authorStatistic.LineCountByFile.Keys)
                {
                    var extension = Path.GetExtension(file);

                    sb.Append("\"");
                    sb.Append(authorStatistic.Author);
                    sb.Append("\";\"");
                    sb.Append(file);
                    sb.Append("\";\"");
                    sb.Append(extension);
                    sb.Append("\";");
                    sb.Append(authorStatistic.LineCountByFile[file]);
                    sb.Append(";");
                    sb.Append(authorStatistic.CommitCount);
                    sb.Append(";");
                    sb.Append(authorStatistic.LineCountByFile.Count);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string GenerateLinesByFileExtension(ICollection<AuthorStatistics> authorStatistics)
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
                    sb.Append(authorStatistic.LineCountByFile.Count);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}