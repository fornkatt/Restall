namespace Restall.Tests.TestUtilities;

internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), "Restall.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public string GetPath(params string[] paths)
    {
        var combined = new string[paths.Length + 1];
        combined[0] = DirectoryPath;
        Array.Copy(paths, 0, combined, 1, paths.Length);
        return Path.Combine(combined);
    }

    public string CreateDirectory(string relativePath)
    {
        var path = GetPath(relativePath);
        Directory.CreateDirectory(path);
        return path;
    }

    public string CreateFile(string relativePath, string contents = "")
    {
        var path = GetPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
                Directory.Delete(DirectoryPath, recursive: true);
        }
        catch
        {
            // Best-effort cleanup for temp files that may be briefly locked by the runtime.
        }
    }
}
