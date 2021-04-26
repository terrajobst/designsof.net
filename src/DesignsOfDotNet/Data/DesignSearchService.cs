using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignsOfDotNet.Data
{
    // TODO: We should react to a web hook whenever the repo is changed.
    public sealed class DesignSearchService
    {
        private readonly DesignService _designService;
        private IReadOnlyList<Design>? _designs;

        public DesignSearchService(DesignService documentService)
        {
            _designService = documentService;
        }

        public async Task<IEnumerable<Design>> SearchAsync(string term)
        {
            if (_designs is null)
                _designs = await _designService.GetDesignsAsync();

            var terms = Tokenize(term);
            return _designs.Where(d => terms.All(t => IsMatch(d, t)))
                           .OrderByDescending(d => d.State)
                           .ThenByDescending(d => d.Year ?? int.MinValue)
                           .ThenBy(d => d.Title);
        }

        private static string[] Tokenize(string term)
        {
            // TODO: Support quotes

            var result = new List<string>();
            var start = -1;

            for (var i = 0; i < term.Length; i++)
            {
                if (!char.IsWhiteSpace(term[i]))
                {
                    if (start < 0)
                        start = i;
                }
                else if (start >= 0 && i > start)
                {
                    var word = term[start..i];
                    result.Add(word);
                    start = -1;
                }
            }

            if (start >= 0 && start < term.Length - 1)
                result.Add(term[start..]);

            return result.ToArray();
        }

        private static bool IsMatch(Design design, string term)
        {
            if (design.Primary is not null && IsMatch(design.Primary, term))
                return true;

            return design.PullRequests.Any(pr => IsMatch(pr, term));
        }

        private static bool IsMatch(DesignPullRequest pr, string term)
        {
            if (IsMatch(pr.Title, term))
                return true;

            if (IsMatch(pr.User, term))
                return true;

            if (IsMatch(pr.Document, term))
                return true;

            return false;
        }

        private static bool IsMatch(Document document, string term)
        {
            if (IsMatch(document.Title, term))
                return true;

            foreach (var owner in document.Owners)
            {
                if (IsMatch(owner.Name, term))
                    return true;

                if (IsMatch(owner.User, term))
                    return true;
            }

            if (IsMatch(document.Contents, term))
                return true;

            return false;
        }

        private static bool IsMatch(string text, string term)
        {
            return text.Contains(term, StringComparison.OrdinalIgnoreCase);
        }
    }
}

