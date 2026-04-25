using System.Text;
using PeNet;
using PeNet.Header.Pe;


namespace Restall.Infrastructure.Helpers;

internal static class PeIconHelper
{
    //Icon Collections
    private const int RtIcon                = 3;
    private const int RtGroupIcon           = 14;
    private const int GrpIconDirHeaderSize  = 6;
    private const int GrpIconDirCountOffset = 4;
    private const int GrpIconEntrySize      = 14;
    private const int GrpIconHeaderSize     = 6;
    private const int FullIconSize          = 256;
    private const uint IcoDataOffset        = 22;

  
    private readonly record struct GrpIconEntry(
        byte Width, byte Height, byte ColorCount,
        ushort Planes, ushort BitCount, ushort Id);

    internal static byte[]? ExtractLargestIconAsPng(string executablePath)
    {
        var fileBytes = File.ReadAllBytes(executablePath);
        var peFile = new PeFile(fileBytes);

        // Step 1: Read RT_GROUP_ICON (14) → raw GRPICONDIR bytes
        var groupDataEntry = peFile.ImageResourceDirectory
            ?.DirectoryEntries
            ?.FirstOrDefault(e => e is { ID: RtGroupIcon, ResourceDirectory: not null })
            ?.ResourceDirectory?.DirectoryEntries?.FirstOrDefault()
            ?.ResourceDirectory?.DirectoryEntries?.FirstOrDefault()
            ?.ResourceDataEntry;

        if (groupDataEntry is null) return null;

        var groupBytes = ReadResourceBytes(peFile, fileBytes, groupDataEntry);
        if (groupBytes is null || groupBytes.Length < GrpIconDirHeaderSize) return null;

        // Step 2: Parse GRPICONDIR — header is 6 bytes, then 14-byte entries
        var count = BitConverter.ToUInt16(groupBytes, GrpIconDirCountOffset);
        if (count == 0) return null;

        var entries = ParseGrpIconEntries(groupBytes, count);
        if (entries.Length == 0) return null;

        // Step 3: Pick largest by area (width/height 0 means 256), then highest bit depth
        var best = entries
            .OrderByDescending(e => (e.Width == 0 ? FullIconSize : e.Width)
                                    * (e.Height == 0 ? FullIconSize : e.Height))
            .ThenByDescending(e => e.BitCount)
            .First();

        // Step 4: Look up RT_ICON (3) by the resource ID from the group entry
        var iconDataEntry = peFile.ImageResourceDirectory
            ?.DirectoryEntries
            ?.FirstOrDefault(e => e is { ID: RtIcon, ResourceDirectory: not null })
            ?.ResourceDirectory?.DirectoryEntries
            ?.FirstOrDefault(e => e?.ID == best.Id)
            ?.ResourceDirectory?.DirectoryEntries?.FirstOrDefault()
            ?.ResourceDataEntry;

        if (iconDataEntry is null) return null;

        // Step 5: Read actual pixel bytes (PNG or BITMAPINFOHEADER+pixels)
        var iconBytes = ReadResourceBytes(peFile, fileBytes, iconDataEntry);
        if (iconBytes is null) return null;

        // Step 6: Already PNG (Vista+ stores 256×256 as PNG) → return directly, otherwise wrap in .ico
        return IsPng(iconBytes) ? iconBytes : BuildIcoStream(iconBytes, best).ToArray();
    }

    private static GrpIconEntry[] ParseGrpIconEntries(byte[] data, ushort count)
    {
        var entries = new List<GrpIconEntry>(count);
        for (int i = 0; i < count; i++)
        {
            int o = GrpIconHeaderSize + i * GrpIconEntrySize;
            if (o + GrpIconEntrySize > data.Length) break;

            entries.Add(new GrpIconEntry(
                Width:      data[o],
                Height:     data[o + 1],
                ColorCount: data[o + 2],
                // data[o + 3] = Reserved
                // data[o + 4..7] = BytesInRes (unused — size sourced from ImageResourceDataEntry.Size1)
                Planes:     BitConverter.ToUInt16(data, o + 4),
                BitCount:   BitConverter.ToUInt16(data, o + 6),
                Id:         BitConverter.ToUInt16(data, o + 12)
            ));
        }
        return entries.ToArray();
    }

    private static byte[]? ReadResourceBytes(PeFile peFile, byte[] fileBytes, ImageResourceDataEntry dataEntry)
    {
        var rva = dataEntry.OffsetToData;
        var section = peFile.ImageSectionHeaders?
            .FirstOrDefault(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + s.SizeOfRawData);
        if (section is null) return null;

        var fileOffset = (int)(rva - section.VirtualAddress + section.PointerToRawData);
        var size       = (int)dataEntry.Size1;

        if (fileOffset + size > fileBytes.Length) return null;
        return fileBytes[fileOffset..(fileOffset + size)];
    }

    private static bool IsPng(byte[] data) =>
        data.Length >= 8 &&
        data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47; // \x89PNG

    private static MemoryStream BuildIcoStream(byte[] iconData, GrpIconEntry entry)
    {
        var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        // ICONDIR (6 bytes)
        writer.Write((ushort)0);  // reserved
        writer.Write((ushort)1);  // type = ICO
        writer.Write((ushort)1);  // image count

        // ICONDIRENTRY (16 bytes)
        writer.Write(entry.Width);
        writer.Write(entry.Height);
        writer.Write(entry.ColorCount);
        writer.Write((byte)0);    // reserved
        writer.Write(entry.Planes);
        writer.Write(entry.BitCount);
        writer.Write((uint)iconData.Length);
        writer.Write((uint)IcoDataOffset); // image data starts at byte 22 (6 + 16)

        writer.Write(iconData);

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}