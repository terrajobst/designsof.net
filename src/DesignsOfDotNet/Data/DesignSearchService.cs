namespace DesignsOfDotNet.Data;

public sealed class DesignSearchService
{
    private readonly DesignService _designService;

    public DesignSearchService(DesignService designService)
    {
        _designService = designService;
    }

    public Task<IEnumerable<Design>> SearchAsync(string term)
    {
        var designs = _designService.Designs;
        var terms = Tokenize(term);
        var result = designs.Where(d => terms.All(t => IsMatch(d, t)))
                            .OrderByDescending(d => d.State)
                            .ThenByDescending(d => d.Year ?? int.MinValue)
                            .ThenBy(d => d.Title);

        return Task.FromResult(result.AsEnumerable());
    }

    private static string[] Tokenize(string term)
    {
        var result = new List<string>();
        var start = -1;
        var inQuote = false;

        for (var i = 0; i < term.Length; i++)
        {
            var c = term[i];

            if (inQuote)
            {
                if (c == '"')
                {
                    inQuote = false;
                    var word = term[start..i];
                    result.Add(word);
                    start = -1;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuote = true;
                    start = i + 1;
                }
                else if (!char.IsWhiteSpace(c))
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
