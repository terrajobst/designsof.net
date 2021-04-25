using System;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignPullRequest
    {
        public DesignPullRequest(int number,
                                 string title,
                                 DateTimeOffset createAt,
                                 string user,
                                 Document document)
        {
            Number = number;
            Title = title;
            CreateAt = createAt;
            User = user;
            Document = document;
        }

        public int Number { get; }
        public string Title { get; }
        public DateTimeOffset CreateAt { get; }
        public string User { get; }
        public string UserUrl => $"https://github.com/{User}";
        public Document Document { get; }
        public string Url => $"https://github.com/dotnet/designs/pull/{Number}";
    }
}

