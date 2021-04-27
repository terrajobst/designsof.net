using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DesignsOfDotNet.Data
{
    public sealed class DesignService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly DesignLoaderService _designService;

        public DesignService(IWebHostEnvironment environment, DesignLoaderService designService)
        {
            _environment = environment;
            _designService = designService;
            Designs = Array.Empty<Design>();
        }

        public DateTime LastRefreshedAt { get; private set; }

        public IReadOnlyList<Design> Designs { get; private set; }

        public async Task UpdateAsync(bool force = false)
        {
            (Designs, LastRefreshedAt) = await LoadDesignsAsync(force);
            DesignsChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task<(IReadOnlyList<Design> Designs, DateTime FetchedAt)> LoadDesignsAsync(bool force)
        {
            if (!force && _environment.IsDevelopment())
            {
                var cacheLocation = GetCacheLocation();
                if (File.Exists(cacheLocation))
                {
                    var lastModified = File.GetLastWriteTime(cacheLocation);
                    using (var stream = File.OpenRead(cacheLocation))
                    {
                        var cachedResult = await JsonSerializer.DeserializeAsync<IReadOnlyList<Design>>(stream);
                        return (cachedResult ?? Array.Empty<Design>(), lastModified);
                    }
                }
            }

            var result = await _designService.LoadAsync();

            if (_environment.IsDevelopment())
            {
                var cacheLocation = GetCacheLocation();
                using (var stream = File.Create(cacheLocation))
                    await JsonSerializer.SerializeAsync(stream, result, new JsonSerializerOptions { WriteIndented = true });
            }

            return (result, DateTime.Now);
        }

        private string GetCacheLocation()
        {
            return Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location!)!, "designs.json");
        }

        public event EventHandler? DesignsChanged;
    }
}

