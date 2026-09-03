using Microsoft.Extensions.Logging;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate
// TODO: LOOK INTO CHANGING UNITY ENGINE DETECTION 
// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class EngineDetectionService : IEngineDetectionService
{   
    private readonly ILogger<EngineDetectionService> _logger;
    public EngineDetectionService(
        ILogger<EngineDetectionService> logger)
    {
        _logger = logger;
    }
    
    public (string? executablePath, Game.Engine engine) DetectExecutablePathAndEngine(string rootPath,
        Game.Platform platform)
    {
        var uePath = FindUEBinariesFolder(rootPath);
        var unityPlayer = FindFileShallow(rootPath, "UnityPlayer.dll", maxDepth: 2);

        Game.Engine engine =
            uePath is not null ? Game.Engine.Unreal :
            unityPlayer is not null ? Game.Engine.Unity :
            Game.Engine.Unknown;

        if (platform == Game.Platform.Xbox) return (rootPath, engine);

        if (uePath is not null) return (uePath, Game.Engine.Unreal);
        return unityPlayer is not null 
            ? (Path.GetDirectoryName(unityPlayer), Game.Engine.Unity) 
            : (FindShallowExeFolder(rootPath), Game.Engine.Unknown);
        
    }


    private string? FindUEBinariesFolder(string? root)
    {
        if (string.IsNullOrEmpty(root)) return null;
        var candidates = new List<string>();
        CollectUEBinaries(root, 0, candidates);
        
        if (candidates.Count == 0) return null;
        
        var withShipping = candidates.FirstOrDefault(c =>
            Directory.GetFiles(c, "*Shipping.exe").Length > 0 ||
            Directory.GetFiles(c, "*.exe").Any(f =>
                Path.GetFileName(f).Contains("Shipping", StringComparison.OrdinalIgnoreCase)));
        
        return withShipping ?? candidates[0];
    }

    private void CollectUEBinaries(string dir, int depth, List<string> results)
    {
        if (depth > 5 || string.IsNullOrEmpty(dir))
        {
            LogUEBinariesScanHitMaxDepth(dir);
            return;
        }
        
        try
        {
            foreach (var sub in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileName(sub);

                if (name.Equals("Engine", StringComparison.OrdinalIgnoreCase)) continue;

                if (name.Equals("Binaries", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var binSub in Directory.GetDirectories(sub))
                    {
                        var binName = Path.GetFileName(binSub);

                        var targetFolder = binName.Equals("Win64", StringComparison.OrdinalIgnoreCase)
                                            || binName.Equals("Win32", StringComparison.OrdinalIgnoreCase)
                                            || binName.Equals("WinGDK", StringComparison.OrdinalIgnoreCase);
                        if (targetFolder && Directory.GetFiles(binSub, "*.exe").Length > 0)
                            results.Add(binSub);
                    }
                    continue;
                }

                CollectUEBinaries(sub, depth + 1, results);
            }
        }
        //TODO: RETURNS THE RESULTS IN THE FUTURE AND CATCH GENERAL EXCEPTIONS IN FACADES AND USECASES
        catch (Exception ex)
        {
            LogFailedToCollectUEBinaries(dir, ex);
        }
    }

    private string? FindFileShallow(string folder, string pattern, int maxDepth)
    {
        if (maxDepth < 0 || !Directory.Exists(folder)) return null;
        try
        {
            var match = Directory.GetFiles(folder, pattern);
            if (match.Length > 0) return match[0];
            if (maxDepth > 0)
                foreach (var sub in Directory.GetDirectories(folder))
                {
                    var filePath = FindFileShallow(sub, pattern, maxDepth - 1);
                    if (filePath is not null)
                    {
                        
                        return filePath;
                    }
                }
        }
        catch (Exception ex)
        {
            LogFailedToFindShallowFiles(folder, ex);
        }

        return null;
    }
    
    private string? FindShallowExeFolder(string root)
    {
        var subFolders = GameScanHelper.GetPreferredExeSubFolders();

        foreach (var sub in subFolders)
        {
            var preferredFolders = Path.Combine(root, sub);
            if (Directory.Exists(preferredFolders) &&
                Directory.GetFiles(preferredFolders, "*.exe").Length > 0)
            {
                return preferredFolders;
            }
        }


        var queue = new Queue<(string path, int depth)>();
        queue.Enqueue((root, 0));
        while (queue.Count > 0)
        {
            var (dir, depth) = queue.Dequeue();
            if (depth > 4)
            {
                LogExecutableFolderNotFound(root);
                continue;
            }
            try
            {
                if (Directory.GetFiles(dir, "*.exe")
                    .Any(f => !GameScanHelper.NonGameExecutable(Path.GetFileNameWithoutExtension(f))))
                {
                    if(depth > 0) 
                        LogFoundExecutableViaBFS(depth, dir);
                    return dir;
                }
                
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var folderName = Path.GetFileName(sub);
                    if (!GameScanHelper.NonGame(folderName))
                        queue.Enqueue((sub, depth + 1));
                }
            }
            catch (Exception ex)
            {
                LogFailedToFindShallowExeFolder(root, ex);
            }
        }
        return null;
    }
}