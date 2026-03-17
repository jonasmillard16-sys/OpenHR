using MudBlazor.Services;
using Microsoft.AspNetCore.Localization;
using RegionHR.Infrastructure;
using RegionHR.Web.Services;
using RegionHR.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Localization (i18n)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "sv", "en" };
    options.SetDefaultCulture("sv");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

// Infrastructure (EF Core, repositories, module contracts)
var connectionString = builder.Configuration.GetConnectionString("RegionHR")
    ?? "Host=localhost;Port=54322;Database=postgres;Username=postgres;Password=postgres";
builder.Services.AddInfrastructure(connectionString);

// Application services
builder.Services.AddScoped<AnstallningService>();
builder.Services.AddScoped<ArendeService>();
builder.Services.AddScoped<SelfServiceApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseRequestLocalization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
