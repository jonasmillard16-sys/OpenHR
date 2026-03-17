using MudBlazor.Services;
using RegionHR.Infrastructure;
using RegionHR.Web.Services;
using RegionHR.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Infrastructure (EF Core, repositories, module contracts)
var connectionString = builder.Configuration.GetConnectionString("RegionHR")
    ?? "Host=localhost;Port=54322;Database=postgres;Username=postgres;Password=postgres";
builder.Services.AddInfrastructure(connectionString);

// Application services
builder.Services.AddScoped<AnstallningService>();
builder.Services.AddScoped<ArendeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
