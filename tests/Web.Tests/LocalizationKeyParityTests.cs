using System.Xml.Linq;

namespace RegionHR.Web.Tests;

/// <summary>
/// Ensures SharedResources.resx (Swedish) and SharedResources.en.resx (English)
/// contain the same set of keys. Any missing translation will fail the test.
/// </summary>
public class LocalizationKeyParityTests
{
    private static readonly string ProjectRoot = FindProjectRoot();

    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "RegionHR.sln")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        // Fallback: walk up from test assembly
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    private static HashSet<string> GetResxKeys(string filePath)
    {
        var doc = XDocument.Load(filePath);
        return doc.Root!
            .Elements("data")
            .Select(e => e.Attribute("name")!.Value)
            .ToHashSet();
    }

    [Fact]
    public void SharedResources_Swedish_And_English_Have_Same_Keys()
    {
        var svPath = Path.Combine(ProjectRoot, "src", "Web", "Resources", "SharedResources.resx");
        var enPath = Path.Combine(ProjectRoot, "src", "Web", "Resources", "SharedResources.en.resx");

        Assert.True(File.Exists(svPath), $"Swedish resource file not found: {svPath}");
        Assert.True(File.Exists(enPath), $"English resource file not found: {enPath}");

        var svKeys = GetResxKeys(svPath);
        var enKeys = GetResxKeys(enPath);

        var missingInEnglish = svKeys.Except(enKeys).OrderBy(k => k).ToList();
        var missingInSwedish = enKeys.Except(svKeys).OrderBy(k => k).ToList();

        Assert.True(missingInEnglish.Count == 0,
            $"Keys in Swedish .resx but missing in English .resx:\n  {string.Join("\n  ", missingInEnglish)}");

        Assert.True(missingInSwedish.Count == 0,
            $"Keys in English .resx but missing in Swedish .resx:\n  {string.Join("\n  ", missingInSwedish)}");
    }

    [Fact]
    public void SharedResources_Has_At_Least_200_Keys()
    {
        var svPath = Path.Combine(ProjectRoot, "src", "Web", "Resources", "SharedResources.resx");
        Assert.True(File.Exists(svPath), $"Swedish resource file not found: {svPath}");

        var svKeys = GetResxKeys(svPath);
        Assert.True(svKeys.Count >= 200,
            $"Expected at least 200 localization keys, found {svKeys.Count}");
    }

    [Fact]
    public void SharedResources_No_Empty_Values()
    {
        var svPath = Path.Combine(ProjectRoot, "src", "Web", "Resources", "SharedResources.resx");
        var enPath = Path.Combine(ProjectRoot, "src", "Web", "Resources", "SharedResources.en.resx");

        foreach (var path in new[] { svPath, enPath })
        {
            Assert.True(File.Exists(path), $"Resource file not found: {path}");
            var doc = XDocument.Load(path);
            var emptyKeys = doc.Root!
                .Elements("data")
                .Where(e => string.IsNullOrWhiteSpace(e.Element("value")?.Value))
                .Select(e => e.Attribute("name")!.Value)
                .ToList();

            Assert.True(emptyKeys.Count == 0,
                $"Empty values in {Path.GetFileName(path)}:\n  {string.Join("\n  ", emptyKeys)}");
        }
    }
}
