using System.Text;

namespace DesignsOfDotNet.Data;

public sealed class Design
{
    public Design(Document? primary, IReadOnlyList<DesignPullRequest> pullRequests)
    {
        if (primary is null && pullRequests.Count == 0)
            throw new ArgumentException("designs with no primary document must have at least one pull request", nameof(pullRequests));

        Primary = primary;
        PullRequests = pullRequests;
    }

    public Document? Primary { get; }
    public IReadOnlyList<DesignPullRequest> PullRequests { get; }

    // Helpers

    public DesignState State =>
        Primary is not null && !PullRequests.Any()
            ? Primary.Kind == DocumentKind.AcceptedDesign ? DesignState.Accepted : DesignState.Draft
            : DesignState.UnderReview;
    public string Title => Primary?.Title ?? PullRequests.First().Document.Title;
    public int? Year => Primary != null ? Primary.Year : PullRequests.First().Document.Year;
    public IReadOnlyList<DocumentOwner> Owners => Primary?.Owners ?? PullRequests.First().Document.Owners;
    public string Url => Primary?.Url ?? PullRequests.First().Url;

    public string StatusText
    {
        get
        {
            var sb = new StringBuilder();

            if (State == DesignState.Accepted)
            {
                sb.Append("Accepted");
            }
            else if (State == DesignState.Draft)
            {
                sb.Append("Draft");
            }
            else
            {
                sb.Append("Under review");
            }

            if (Year != null)
            {
                sb.Append(", authored in ");
                sb.Append(Year);
            }

            if (Owners.Any())
            {
                if (Year == null)
                    sb.Append(", authored ");

                sb.Append(" by ");
                var isFirst = true;
                foreach (var owner in Owners)
                {
                    if (isFirst)
                        isFirst = false;
                    else if (Owners.Count == 2)
                        sb.Append(" and ");
                    else if (owner == Owners.Last())
                        sb.Append(", and ");
                    else
                        sb.Append(", ");

                    sb.Append(owner);
                }
            }

            return sb.ToString();
        }
    }
}
