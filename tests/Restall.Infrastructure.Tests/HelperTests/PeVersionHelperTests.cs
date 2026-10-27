// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Tests.HelperTests;

public class PeVersionHelperTests : IDisposable
{
    private readonly string _tempDir = Directory.CreateTempSubdirectory("restall-pe-version-tests").FullName;

    private static string InfrastructureAssemblyPath => typeof(PeVersionHelper).Assembly.Location;

    public void Dispose() => Directory.Delete(_tempDir, true);

    [Fact]
    public void GetVersionInfo_ManagedAssembly_ReturnsVersionStrings()
    {
        var actual = PeVersionHelper.GetVersionInfo(InfrastructureAssemblyPath);

        Assert.NotNull(actual);
        Assert.Equal("Restall.Infrastructure.dll", actual.OriginalFilename);
        Assert.Equal("Restall", actual.ProductName);
    }

    [Fact]
    public void GetVersionInfo_EmptyFile_ReturnsNothing()
    {
        var filePath = Path.Combine(_tempDir, "empty.dll");
        File.WriteAllBytes(filePath, []);

        var actual = PeVersionHelper.GetVersionInfo(filePath);

        Assert.Null(actual);
    }

    [Fact]
    public void GetVersionInfo_TruncatedPeFile_ReturnsNothing()
    {
        var filePath = Path.Combine(_tempDir, "truncated.dll");
        File.WriteAllBytes(filePath, File.ReadAllBytes(InfrastructureAssemblyPath)[..64]);

        var actual = PeVersionHelper.GetVersionInfo(filePath);

        Assert.Null(actual);
    }
}
