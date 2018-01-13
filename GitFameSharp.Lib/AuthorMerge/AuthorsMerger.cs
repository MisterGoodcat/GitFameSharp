using System.Collections.Generic;
using System.Linq;
using GitFameSharp.Git;

namespace GitFameSharp.AuthorMerge
{
    public sealed class AuthorsMerger
    {
        private readonly AuthorMergeOptions _options;

        public AuthorsMerger(AuthorMergeOptions options)
        {
            _options = options;
        }

        public ICollection<AuthorStatistics> Merge(ICollection<AuthorStatistics> authorStatistics)
        {
            var processedAuthors = new List<string>();
            var result = new List<AuthorStatistics>();

            foreach (var authorsToMergeEntry in _options.AuthorsToMerge)
            {
                var existingAuthorStatistics = authorStatistics.Where(x => authorsToMergeEntry.Value.Contains(x.Author)).ToList();
                var newName = authorsToMergeEntry.Key;
                var newAuthorStatistics = AuthorStatistics.MergeFrom(newName, existingAuthorStatistics);
                result.Add(newAuthorStatistics);

                processedAuthors.AddRange(authorsToMergeEntry.Value);
            }

            var unprocessedAuthors = authorStatistics.Where(x => !processedAuthors.Contains(x.Author)).ToList();
            return result.Concat(unprocessedAuthors).ToList();
        }
    }
}