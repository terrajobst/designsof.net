using System;
using System.Collections.Generic;
using System.Linq;

namespace DesignsOfDotNet.Data
{
    public sealed class Design
    {
        public Design(Document? primary, IReadOnlyList<DesignPullRequest> pullRequests)
        {
            if (primary is null && pullRequests.Count == 0)
                throw new ArgumentException(nameof(pullRequests), "designs with no primary document must have at least one pull request");

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
    }
}

