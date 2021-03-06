using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitFameSharp.Git
{
    public sealed class GitCommands
    {
        private const string GitExecutable = "git";
        private readonly string[] _defaultCommandLineArgs;
        private readonly GitOptions _gitOptions;
        private readonly Regex _includeRegex;
        private readonly Regex _excludeRegex;

        public GitCommands(GitOptions gitOptions)
        {
            _gitOptions = gitOptions;
            _defaultCommandLineArgs = new[] { "-C", _gitOptions.GitDir };
            _includeRegex = !string.IsNullOrWhiteSpace(_gitOptions.Include) ? new Regex(_gitOptions.Include, RegexOptions.IgnoreCase) : null;
            _excludeRegex = !string.IsNullOrWhiteSpace(_gitOptions.Exclude) ? new Regex(_gitOptions.Exclude, RegexOptions.IgnoreCase) : null;
        }

        public async Task<ICollection<string>> GetFilesAsync()
        {
            var result = await ExecuteGitAsync("ls-files", "--with-tree", _gitOptions.Branch).ConfigureAwait(false);
            var filteredResult = FilterFiles(result);
            return filteredResult;
        }

        public async Task<Dictionary<string, int>> ShortlogAsync()
        {
            var lines = await ExecuteGitAsync("shortlog", "-s", _gitOptions.Branch).ConfigureAwait(false);
            var result = ProcessRawCommitCount(lines);
            return result;
        }

        public async Task<FileStatistics> BlameAsync(string file)
        {
            const string authorMarker = "author ";
            var result = await ExecuteGitAsync(s => s?.StartsWith(authorMarker) ?? false, "blame", "--line-porcelain", _gitOptions.Branch, file).ConfigureAwait(false);
            var authors = result
                .Select(x => x.Substring(authorMarker.Length).Trim())
                .ToList()
                .Aggregate(new Dictionary<string, int>(), (dict, author) =>
                {
                    dict.TryGetValue(author, out var count);
                    dict[author] = count + 1;
                    return dict;
                });
            
            return new FileStatistics(file)
            {
                Authors = authors.Keys,
                LinesCountByAuthor = authors
            };
        }

        private ICollection<string> FilterFiles(ICollection<string> result)
        {
            IEnumerable<string> filteredResult = result.Where(x => !string.IsNullOrWhiteSpace(x));

            if (_excludeRegex != null)
            {
                filteredResult = filteredResult.Where(x => !_excludeRegex.IsMatch(x));
            }

            if (_includeRegex != null)
            {
                filteredResult = filteredResult.Where(x => _includeRegex.IsMatch(x));
            }

            return filteredResult.ToArray();
        }
        
        private Dictionary<string, int> ProcessRawCommitCount(ICollection<string> lines)
        {
            var result = lines
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Contains("\t"))
                .Select(x => x.Split(new [] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(x => x[1], x => int.Parse(x[0].Trim()));

            return result;
        }

        private Task<ICollection<string>> ExecuteGitAsync(params string[] args)
        {
            return ExecuteGitAsync(null, args);
        }
        
        private Task<ICollection<string>> ExecuteGitAsync(Func<string, bool> filterFunc, params string[] args)
        {
            var psi = new ProcessStartInfo(GitExecutable)
            {
                Arguments = string.Join(" ", _defaultCommandLineArgs.Concat(args)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false
            };

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            var lines = new List<string>();
            var taskCompletionSource = new TaskCompletionSource<ICollection<string>>();

            process.Start();
            process.BeginOutputReadLine();
            process.OutputDataReceived += (o, e) =>
            {
                if (filterFunc != null && !filterFunc(e.Data))
                {
                    return;
                }

                lines.Add(e.Data);
            };

            process.ErrorDataReceived += (o, e) =>
            {
                // TODO?
                Console.WriteLine("ERROR: " + e.Data);
            };

            process.Exited += (o, e) =>
            {
                taskCompletionSource.SetResult(lines);
                process.Dispose();
            };

            return taskCompletionSource.Task;
        }
    }
}