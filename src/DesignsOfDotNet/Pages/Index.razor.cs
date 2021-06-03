using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DesignsOfDotNet.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace DesignsOfDotNet.Pages
{
    public sealed partial class Index : IDisposable
    {
        private string _searchText = string.Empty;
        private ElementReference _searchInputReference;

        [Inject]
        public IWebHostEnvironment Environment { get; set; } = null!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public DesignService DesignService { get; set; } = null!;

        [Inject]
        public DesignSearchService DesignSearchService { get; set; } = null!;

        public IEnumerable<Design> SearchResults { get; set; } = Array.Empty<Design>();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    UpdateSearchResults();
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await _searchInputReference.FocusAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            DesignService.DesignsChanged += DesignService_DesignsChanged;
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("q", out var q))
                SearchText = q;
            else
                SearchResults = await DesignSearchService.SearchAsync("");
        }

        public void Dispose()
        {
            DesignService.DesignsChanged -= DesignService_DesignsChanged;
        }

        private void DesignService_DesignsChanged(object? sender, EventArgs e)
        {
            InvokeAsync(() => UpdateSearchResults());
        }

        private async void UpdateSearchResults()
        {
            SearchResults = await DesignSearchService.SearchAsync(SearchText);

            var queryString = string.IsNullOrEmpty(SearchText)
                ? string.Empty
                : "?q=" + SearchText;

            var uri = new UriBuilder(NavigationManager.Uri)
            {
                Query = queryString
            }.ToString();

            await JSRuntime.InvokeAsync<object>("changeUrl", uri);
            StateHasChanged();
        }
    }
}
