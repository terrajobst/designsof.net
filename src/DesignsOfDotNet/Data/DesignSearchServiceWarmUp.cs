using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignSearchServiceWarmUp : IHostedService
    {
        private readonly DesignSearchService _designSearchService;

        public DesignSearchServiceWarmUp(DesignSearchService designSearchService)
        {
            _designSearchService = designSearchService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _designSearchService.SearchAsync(string.Empty);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

