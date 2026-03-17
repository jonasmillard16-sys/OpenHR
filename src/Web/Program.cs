using MudBlazor.Services;
using Microsoft.AspNetCore.Localization;
using RegionHR.Infrastructure;
using RegionHR.Web.Hubs;
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

// Swedish culture for datepickers
var svCulture = new System.Globalization.CultureInfo("sv-SE");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = svCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = svCulture;

// Infrastructure (EF Core, repositories, module contracts)
var connectionString = builder.Configuration.GetConnectionString("RegionHR")
    ?? "Host=localhost;Port=54322;Database=postgres;Username=postgres;Password=postgres";
builder.Services.AddInfrastructure(connectionString);

// SignalR
builder.Services.AddSignalR();

// Application services
builder.Services.AddScoped<AnstallningService>();
builder.Services.AddScoped<ArendeService>();
builder.Services.AddScoped<SelfServiceApiClient>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<AuthService>();

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

app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
