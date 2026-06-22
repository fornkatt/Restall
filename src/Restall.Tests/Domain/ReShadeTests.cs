using Restall.Domain.Entities;

namespace Restall.Tests.Domain;

public sealed class ReShadeTests
{
    // Verifies that ReShade filename options combine with dll and asi extensions correctly.
    [Theory]
    [InlineData(ReShade.Filename.Dxgi, ReShade.FileExtension.Dll, "dxgi.dll")]
    [InlineData(ReShade.Filename.D3d12, ReShade.FileExtension.Dll, "d3d12.dll")]
    [InlineData(ReShade.Filename.Version, ReShade.FileExtension.Asi, "version.asi")]
    [InlineData(ReShade.Filename.ReShade64, ReShade.FileExtension.Asi, "ReShade64.asi")]
    public void GetFileName_CombinesFilenameAndExtension(
        ReShade.Filename filename,
        ReShade.FileExtension extension,
        string expected)
    {
        var result = ReShade.GetFileName(filename, extension);

        Assert.Equal(expected, result);
    }

    // Verifies that x64 ReShade installers expose the expected original DLL name.
    [Fact]
    public void OriginalFileName_WhenArchitectureIsX64_ReturnsReShade64Dll()
    {
        var reShade = new ReShade { Arch = ReShade.Architecture.x64 };

        Assert.Equal("ReShade64.dll", reShade.OriginalFileName);
    }

    // Verifies that x32 ReShade installers expose the expected original DLL name.
    [Fact]
    public void OriginalFileName_WhenArchitectureIsX32_ReturnsReShade32Dll()
    {
        var reShade = new ReShade { Arch = ReShade.Architecture.x32 };

        Assert.Equal("ReShade32.dll", reShade.OriginalFileName);
    }
}
