using System.Collections.Generic;

namespace GitFameSharp.AuthorMerge
{
    public sealed class AuthorMergeOptions
    {
        /// <summary>
        /// Determines lists of author aliases which should be merged using a final author name.
        /// </summary>
        public IDictionary<string, ICollection<string>> AuthorsToMerge { get; set; }

        public AuthorMergeOptions(Dictionary<string, ICollection<string>> authorsToMergeLookup)
        {
            AuthorsToMerge = authorsToMergeLookup;
        }
    }
}