namespace GitFameSharp.Git
{
    public sealed class GitOptions
    {
        public string GitDir { get; set; }
        public string Branch { get; set; }
        public string Include { get; set; }
        public string Exclude { get; set; }
    }
}
