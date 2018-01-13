using System.Collections.Generic;

namespace GitFameSharp.Git
{
    public sealed class FileStatistics
    {
        public string File { get; }
        public IEnumerable<string> Authors { get; set; }
        public Dictionary<string, int> LinesCountByAuthor { get; set; }

        public FileStatistics(string file)
        {
            File = file;
        }

        public override string ToString()
        {
            return File;
        }
    }
}