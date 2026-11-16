namespace TextBasedRPG.Tests.Support;

/// <summary>Finds files in the repository from a running test.</summary>
internal static class Repo
{
    /// <summary>The repository root: the first parent folder that has <c>global.json</c>.</summary>
    internal static string Root
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(System.IO.Path.Combine(dir.FullName, "global.json")))
                dir = dir.Parent;

            return dir?.FullName ?? throw new InvalidOperationException("Repo root (global.json) not found");
        }
    }

    internal static string Path(params string[] parts) => System.IO.Path.Combine([Root, .. parts]);

    internal static string Read(params string[] parts) => File.ReadAllText(Path(parts));
}
