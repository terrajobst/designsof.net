using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Markdig;

namespace DesignsOfDotNet.Data
{
    public sealed class Document
    {
        public Document(DocumentKind kind, string url, string path, int? year, string title, IReadOnlyList<DocumentOwner> owners, string contents)
        {
            Kind = kind;
            Url = url;
            Path = path;
            Year = year;
            Title = title;
            Contents = contents;
            Owners = owners.ToArray();
        }

        public DocumentKind Kind { get; }
        public string Url { get; }
        public string Path { get; }
        public int? Year { get; }
        public string Title { get; }
        public IReadOnlyList<DocumentOwner> Owners { get; }
        public string Contents { get; }

        public static Document? Parse(string url, string path, string contents)
        {
            var segments = url.Split('/');

            var kind = (DocumentKind?)null;
            var year = (int?)null;
            var title = (string?)null;
            var owners = new List<DocumentOwner>();

            foreach (var segment in segments.Reverse())
            {
                if (string.Equals(segment, "meta", StringComparison.OrdinalIgnoreCase))
                {
                    kind = DocumentKind.Meta;
                    break;
                }
                else if (string.Equals(segment, "accepted", StringComparison.OrdinalIgnoreCase))
                {
                    kind = DocumentKind.AcceptedDesign;
                    break;
                }
                else if (string.Equals(segment, "proposed", StringComparison.OrdinalIgnoreCase))
                {
                    kind = DocumentKind.ProposedDesign;
                    break;
                }

                if (int.TryParse(segment, out var number))
                {
                    year = number;
                }
            }

            if (kind is null)
                return null;

            static IEnumerable<string> GetLines(string contents)
            {
                string? line;
                using var sr = new StringReader(contents);
                while ((line = sr.ReadLine()) is not null)
                    yield return line;
            }

            // Extract titles, owners, and draft status from content above any subheadings
            var subheadingRegex = new Regex("^#{2,}");
            var reachedSubheading = false;

            var titleRegex = new Regex("^# *(?<title>.*?)#?$");
            var ownerRegex = new Regex(@"^\*\*Owner(s)?\*\*(?<owner>[^|,]+)(\s*[\|,]\s*(?<owner>[^|,]+))*", RegexOptions.IgnoreCase);
            var draftRegex = new Regex(@"^\*\*DRAFT\*\*$", RegexOptions.IgnoreCase);

            foreach (var line in GetLines(contents))
            {
                reachedSubheading = reachedSubheading || subheadingRegex.Match(line).Success;

                if (!reachedSubheading)
                {
                    var titleMatch = titleRegex.Match(line);
                    var ownerMatch = ownerRegex.Match(line);
                    var draftMatch = draftRegex.Match(line);

                    if (titleMatch.Success && title is null)
                    {
                        title = Markdown.ToPlainText(titleMatch.Groups["title"].Value.Trim());
                    }
                    else if (ownerMatch.Success)
                    {
                        foreach (Capture capture in ownerMatch.Groups["owner"].Captures)
                        {
                            var ownerText = capture.Value.Trim();
                            if (ownerText.Length > 0)
                            {
                                var owner = DocumentOwner.Parse(ownerText);
                                if (owner is not null)
                                    owners.Add(owner);
                            }
                        }
                    }
                    else if (draftMatch.Success)
                    {
                        kind = DocumentKind.DraftDesign;
                    }
                }
            }

            if (title is null)
                return null;

            // Some designs have sub designs. We could have a marker in the document
            // but for now it's easier to say "if there is no explicit PM/dev marker"
            // we assume it's a sub design and return null.

            if (kind == DocumentKind.AcceptedDesign && !owners.Any())
                return null;

            return new Document(kind.Value, url, path, year, title, owners.ToArray(), contents);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

