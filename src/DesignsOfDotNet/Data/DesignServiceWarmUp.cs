using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignServiceWarmUp : IHostedService
    {
        private readonly DesignService _designService;

        public DesignServiceWarmUp(DesignService designService)
        {
            _designService = designService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _designService.UpdateAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

