using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class EngineDetectionService : IEngineDetectionService
{
    private readonly ILogService _logService;

    public EngineDetectionService(ILogService logService)
    {
        _logService = logService;
    }
    
    public (string? executablePath, Game.Engine engine) DetectExecutablePathAndEngine(string rootPath)
    {
        var uePath = FindUEBinariesFolder(rootPath);
        if (uePath != null)
        {
            return (uePath, Game.Engine.Unreal);
        }
    
        var unityPlayer = FindFileShallow(rootPath, "UnityPlayer.dll", maxDepth: 2);
        if (unityPlayer != null)
        {
            return (Path.GetDirectoryName(unityPlayer), Game.Engine.Unity);
        }
    
        var exeFolder = FindShallowExeFolder(rootPath);
        return (exeFolder, Game.Engine.Unknown);
    }
    
    

    private string? FindUEBinariesFolder(string? root)
    {
        if (string.IsNullOrEmpty(root)) return null;
        var candidates = new List<string>();
        CollectUEBinaries(root, 0, candidates);
        if (candidates.Count == 0) return null;

        // Prefer folders that contain a Shipping exe
        var withShipping = candidates.FirstOrDefault(c =>
            Directory.GetFiles(c, "*Shipping.exe").Length > 0 ||
            Directory.GetFiles(c, "*.exe").Any(f =>
                Path.GetFileName(f).Contains("Shipping", StringComparison.OrdinalIgnoreCase)));

        return withShipping ?? candidates[0];
    }

    private void CollectUEBinaries(string dir, int depth, List<string> results)
    {
        if (depth > 5 || string.IsNullOrEmpty(dir)) return;

        try
        {
            foreach (var sub in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileName(sub);
                    
                // Skip Engine folder — its Binaries are for the engine, not the game
                if (name.Equals("Engine", StringComparison.OrdinalIgnoreCase)) continue;

                // Found a Binaries folder — look inside for Win64/WinGDK
                if (name.Equals("Binaries", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var binSub in Directory.GetDirectories(sub))
                    {
                        var binName = Path.GetFileName(binSub);
                        
                            bool targetFolder = binName.Equals("Win64", StringComparison.OrdinalIgnoreCase)
                                                   || binName.Equals("WinGDK", StringComparison.OrdinalIgnoreCase);
                            if (targetFolder && Directory.GetFiles(binSub, "*.exe").Length > 0)
                                results.Add(binSub);
                        
                        
                    }

                    // Don't recurse further into Binaries
                    continue;
                }

                // Recurse into non-Engine, non-Binaries subfolders
                CollectUEBinaries(sub, depth + 1, results);
            }
        }
        catch(Exception ex)
        {
            _logService.LogError($"Couldn't collect the files in Binaries folder", ex);
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
                    if (filePath is not null) return filePath;
                }
        }
        catch (Exception ex)
        {
            _logService.LogError($" Failed the find shallow files", ex);
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
                return preferredFolders;
        }


        var queue = new Queue<(string path, int depth)>();
        queue.Enqueue((root, 0));
        while (queue.Count > 0)
        {
            var (dir, depth) = queue.Dequeue();
            if (depth > 4) continue;
            try
            {
                if (Directory.GetFiles(dir, "*.exe").Length > 0) return dir;
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var folderName = Path.GetFileName(sub);
                    if (!GameScanHelper.NonGame(folderName))
                        queue.Enqueue((sub, depth + 1));
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($" Couldn't find any EXE files in Folders:",ex);
            }
        }

        return null;
    }

}