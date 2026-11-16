using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TextBasedRPG.Tests.Support;

namespace TextBasedRPG.Tests;

/// <summary>
/// release.yml checks tag vX.Y.Z against the csproj &lt;Version&gt; X.Y.Z.0. These tests catch a bad version before tagging.
/// </summary>
public class ReleaseVersionTests
{
    private static string CsprojVersion()
    {
        var csproj = XDocument.Load(Repo.Path("TextBasedRPG", "TextBasedRPG.csproj"));
        return csproj.Descendants("Version").Single().Value.Trim();
    }

    [Fact]
    public void Csproj_version_matches_the_tag_format()
    {
        Assert.Matches(new Regex(@"^\d+\.\d+\.\d+\.0$"), CsprojVersion());
    }

    [Fact]
    public void Csproj_version_is_not_older_than_the_published_version()
    {
        var repoJson = Repo.Path("repo.json");
        if (!File.Exists(repoJson))
            return; // Nothing released yet; the first release creates repo.json.

        using var doc = JsonDocument.Parse(File.ReadAllText(repoJson));
        var published = Version.Parse(doc.RootElement[0].GetProperty("AssemblyVersion").GetString()!);
        var csproj = Version.Parse(CsprojVersion());

        Assert.True(csproj >= published,
            $"TextBasedRPG.csproj says {csproj}, but repo.json already publishes {published}. Bump <Version> in the csproj.");
    }

    [Fact]
    public void Release_workflow_uses_this_projects_paths()
    {
        var workflow = Repo.Read(".github", "workflows", "release.yml");
        Assert.Contains("TextBasedRPG/TextBasedRPG.csproj", workflow);
        Assert.Contains("TextBasedRPG/bin/x64/Release/TextBasedRPG/latest.zip", workflow);
        Assert.Contains("TextBasedRPG/bin/x64/Release/TextBasedRPG/TextBasedRPG.json", workflow);
    }
}
