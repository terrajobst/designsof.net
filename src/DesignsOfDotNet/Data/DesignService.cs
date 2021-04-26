using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignService
    {
        private readonly DesignLoaderService _designService;

        public DesignService(DesignLoaderService designService)
        {
            _designService = designService;
            Designs = Array.Empty<Design>();
        }

        public DateTime LastRefreshedAt { get; private set; }

        public IReadOnlyList<Design> Designs { get; private set; }

        public async Task UpdateAsync()
        {
            Designs = await _designService.LoadAsync();
            LastRefreshedAt = DateTime.Now;
            DesignsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? DesignsChanged;
    }
}

