using System;
using System.Reflection.Metadata.Ecma335;

namespace GitFameSharp
{
    public class Options
    {
        public string GitDir { get; set; } = ".";
        public string Branch { get; set; } = "HEAD";
        public string Include { get; set; }
        public string Exclude { get; set; }
        public int ParallelBlameProcesses { get; set; } = Environment.ProcessorCount;
        public string Output { get; set; } = "result.csv";
        public string AuthorsToMerge { get; set; }
    }
}