using System.Text.Json;

namespace RegionHR.Web.Services;

public sealed class GlossaryService
{
    private Dictionary<string, string> _terms = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public async Task EnsureLoadedAsync(HttpClient http)
    {
        if (_loaded) return;
        try
        {
            var json = await http.GetStringAsync("data/glossary-sv.json");
            _terms = JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { _terms = new(); }
        _loaded = true;
    }

    public string? GetDefinition(string term) =>
        _terms.TryGetValue(term, out var def) ? def : null;

    public IReadOnlyDictionary<string, string> GetAllTerms() => _terms;
}
