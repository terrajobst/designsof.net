using DesignsOfDotNet.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddSingleton<DesignLoaderService>();
builder.Services.AddSingleton<DesignService>();
builder.Services.AddSingleton<DesignSearchService>();
builder.Services.AddSingleton<GitHubClientFactory>();

var app = builder.Build();

// Warm up services
var designService = app.Services.GetRequiredService<DesignService>();
await designService.UpdateAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapDefaultControllerRoute();
app.MapFallbackToPage("/_Host");

app.Run();
