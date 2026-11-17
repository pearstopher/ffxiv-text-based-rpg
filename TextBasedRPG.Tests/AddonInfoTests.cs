using System.Text.Json;
using TextBasedRPG.Core;

namespace TextBasedRPG.Tests;

public class AddonInfoTests
{
    [Fact]
    public void Json_lists_addons_sorted_by_name_with_build_and_time()
    {
        var json = AddonInfo.ToJson(
            [new("_NaviMap", true, true, 4), new("ChatLog", true, true, 5), new("_ActionBar", false, true, 4)],
            new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero),
            "Debug build");

        using var doc = JsonDocument.Parse(json);
        var names = doc.RootElement.GetProperty("Addons").EnumerateArray().Select(a => a.GetProperty("Name").GetString()).ToList();
        Assert.Equal(["ChatLog", "_ActionBar", "_NaviMap"], names);
        Assert.Equal("Debug build", doc.RootElement.GetProperty("Build").GetString());
        Assert.False(doc.RootElement.GetProperty("Addons")[1].GetProperty("IsVisible").GetBoolean());
    }
}
