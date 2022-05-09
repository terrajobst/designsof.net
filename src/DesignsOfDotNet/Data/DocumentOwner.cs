using System.Text.RegularExpressions;

namespace DesignsOfDotNet.Data;

public sealed class DocumentOwner
{
    public DocumentOwner(string name, string user)
    {
        Name = name;
        User = user;
    }

    public string Name { get; }
    public string User { get; }
    public string Url => $"https://github.com/{User}";
    public string AvatarUrl => $"{Url}.png";

    public static DocumentOwner? Parse(string text)
    {
        var match = Regex.Match(text, @"\[(?<name>[^]]+)\]\(https://github.com/(?<user>.*)\)");
        if (!match.Success)
            return null;

        var name = match.Groups["name"].Value;
        var user = match.Groups["user"].Value;
        return new DocumentOwner(name, user);
    }

    public override string ToString()
    {
        return Name;
    }
}