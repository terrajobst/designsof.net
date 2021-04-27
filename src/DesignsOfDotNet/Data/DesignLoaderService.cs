using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Octokit;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignLoaderService
    {
        private readonly GitHubClientFactory _gitHubClientFactory;

        public DesignLoaderService(GitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task<IReadOnlyList<Design>> LoadAsync()
        {
            var gitHub = await _gitHubClientFactory.CreateAsync();

            var documentByPath = new Dictionary<string, Document>();

            foreach (var document in await GetDocuments(DesignsOfDotNetConstants.DesignsOwner, DesignsOfDotNetConstants.DesignsRepo, DesignsOfDotNetConstants.DesignsBranch))
                documentByPath.Add(document.Path, document);

            var request = new PullRequestRequest()
            {
                State = ItemStateFilter.Open
            };

            var pullRequests = await gitHub.Repository.PullRequest.GetAllForRepository(DesignsOfDotNetConstants.DesignsOwner, DesignsOfDotNetConstants.DesignsRepo, request);
            var designPullRequests = new List<DesignPullRequest>();

            foreach (var pr in pullRequests)
            {
                var owner = pr.Head.Repository.Owner.Login;
                var repo = pr.Head.Repository.Name;
                var branch = pr.Head.Ref;
                var documents = await GetDocuments(owner, repo, branch);
                var prFiles = await gitHub.Repository.PullRequest.Files(DesignsOfDotNetConstants.DesignsOwner, DesignsOfDotNetConstants.DesignsRepo, pr.Number);
                var prFileNames = prFiles.Select(prf => prf.FileName)
                                         .ToHashSet();

                var prDocuments = documents.Where(d => prFileNames.Contains(d.Path));

                foreach (var document in prDocuments)
                {
                    var designPullRequest = new DesignPullRequest(pr.Number,
                        pr.Title, pr.CreatedAt, pr.User.Login, document);
                    designPullRequests.Add(designPullRequest);
                }
            }

            var designByPath = new Dictionary<string, (Document? Document, List<DesignPullRequest> PRs)>();

            foreach (var document in documentByPath.Values)
            {
                designByPath.Add(document.Path, (document, new List<DesignPullRequest>()));
            }

            foreach (var pr in designPullRequests)
            {
                if (!designByPath.TryGetValue(pr.Document.Path, out var design))
                {
                    design = (null, new List<DesignPullRequest>());
                    designByPath.Add(pr.Document.Path, design);
                }

                design.PRs.Add(pr);
            }

            return designByPath.Values.Select(d => new Design(d.Document, d.PRs.ToArray())).ToArray();
        }

        private static async Task<IReadOnlyList<Document>> GetDocuments(string owner, string repo, string branch)
        {
            var zipUrl = $"https://github.com/{owner}/{repo}/archive/refs/heads/{branch}.zip";
            var urlPrefix = $"https://github.com/{owner}/{repo}/blob/{branch}/";

            var result = new List<Document>();

            using (var memoryStream = new MemoryStream())
            {
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(zipUrl))
                    await stream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var markdownFiles = archive.Entries.Where(e => string.Equals(Path.GetExtension(e.Name), ".md", StringComparison.OrdinalIgnoreCase));

                    foreach (var entry in archive.Entries)
                    {
                        var isMarkdown = string.Equals(Path.GetExtension(entry.Name), ".md", StringComparison.OrdinalIgnoreCase);
                        if (!isMarkdown)
                            continue;

                        using (var streamReader = new StreamReader(entry.Open()))
                        {
                            var contents = streamReader.ReadToEnd();

                            // Note: The first segment is the name of the repo and branch,
                            //       so we clip that off.
                            var path = string.Join('/', entry.FullName.Split('/').Skip(1));
                            var url = urlPrefix + path;
                            var document = Document.Parse(url, path, contents);
                            if (document is not null && document.Kind != DocumentKind.Meta)
                                result.Add(document);
                        }
                    }
                }
            }

            return result.ToArray();
        }
    }
}

