// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Tests.HelperTests;

public class PeFileParserTests
{
    private static string InfrastructureAssemblyPath => typeof(PeFileParser).Assembly.Location;

    [Fact]
    public void Parse_ManagedAssembly_ReturnsPeFile()
    {
        using var stream = File.OpenRead(InfrastructureAssemblyPath);

        var actual = PeFileParser.Parse(stream);

        Assert.NotNull(actual);
    }

    [Fact]
    public void Parse_EmptyStream_ReturnsNothing()
    {
        using var stream = new MemoryStream([]);

        var actual = PeFileParser.Parse(stream);

        Assert.Null(actual);
    }

    [Fact]
    public void Parse_TruncatedPeFile_ReturnsNothing()
    {
        using var stream = new MemoryStream(File.ReadAllBytes(InfrastructureAssemblyPath)[..64]);

        var actual = PeFileParser.Parse(stream);

        Assert.Null(actual);
    }
}
