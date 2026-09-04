using System.Text;
using PeNet;

namespace Restall.Infrastructure.Helpers;

// TODO: wrap every return null path in Result to get meaningful information up to caller for the log  
//TODO: LOOK UP HOW I CAN BYPASS ICONHELPER WITH TRY/CATCH DUE TO PENET STANDARD
internal static class PeIconHelper
{
    private const int RtIcon = 3;
    private const int RtGroupIcon = 14;
    
    private const int GrpCountOffset = 4;
    private const int GrpHeaderSize = 6;
    private const int GrpEntrySize = 14;
    private const int GrpWidthOffset = 0;
    private const int GrpHeightOffset = 1;
    private const int GrpBitCountOffset = 6;
    private const int GrpIdOffset = 12;
    private const int SizeOf256 = 256;
    
    private const byte PngMagic0 = 0x89;
    private const byte PngMagic1 = 0x50; 
    private const byte PngMagic2 = 0x4E; 
    private const byte PngMagic3 = 0x47;

    private const int BmpWidthOffset = 4;
    private const int BmpHeightOffset = 8;

    private const ushort IcoReserved = 0;
    private const ushort IcoTypeIcon = 1;
    private const int IcoDirHeaderSize = 6;
    private const int IcoDirEntrySize = 16;
    
    internal static byte[]? ExtractLargestIconAsPng(string executablePath)
    {
        var fileBytes = File.ReadAllBytes(executablePath);
        var pe = new PeFile(fileBytes);
        
         var groupBytes = ReadResource(pe,fileBytes,typeId: RtGroupIcon, resourceId: null);
         if (groupBytes is null || groupBytes.Length < GrpHeaderSize) return null;

         int count = BitConverter.ToUInt16(groupBytes, GrpCountOffset);

         if (count == 0) return null;
         
         var bestId = Enumerable.Range(0, count)
             .Select(i => GrpHeaderSize + i * GrpEntrySize)
             .Where(o => o + GrpEntrySize <= groupBytes.Length)
             .Select(o => (
                 Area:     (groupBytes[o + GrpWidthOffset]  == 0 ? SizeOf256 : groupBytes[o + GrpWidthOffset]) *
                           (groupBytes[o + GrpHeightOffset] == 0 ? SizeOf256 : groupBytes[o + GrpHeightOffset]),
                 BitCount: BitConverter.ToUInt16(groupBytes, o + GrpBitCountOffset),
                 Id:       BitConverter.ToUInt16(groupBytes, o + GrpIdOffset)))
             .OrderByDescending(e => e.Area)
             .ThenByDescending(e => e.BitCount)
             .FirstOrDefault().Id;
         if (bestId == 0) return null;
         
         var iconBytes = ReadResource(pe, fileBytes, typeId: RtIcon, resourceId: bestId);
         if (iconBytes is null) return null;
         
         return IsPng(iconBytes) ? iconBytes : WrapInIco(iconBytes);
    }
    
    private static byte[]? ReadResource(PeFile pe, byte[] fileBytes, int typeId, ushort? resourceId)
    {
        var resourceType = pe.ImageResourceDirectory?.DirectoryEntries?
            .FirstOrDefault(e => e?.ID == typeId)?
            .ResourceDirectory;
        
        var resourceEntry = resourceId is null
            ? resourceType?.DirectoryEntries?.FirstOrDefault()
            : resourceType?.DirectoryEntries?.FirstOrDefault(e => e!.ID == resourceId);

        var languageData = resourceEntry?.ResourceDirectory?.DirectoryEntries?.FirstOrDefault()?.ResourceDataEntry;
        if (languageData is null) return null;
        
        var resourceSection = pe.ImageSectionHeaders?.FirstOrDefault(s => 
            languageData.OffsetToData >= s.VirtualAddress && 
            languageData.OffsetToData < s.VirtualAddress + s.SizeOfRawData);
        
        if(resourceSection is null) return null;
        
        var offset = (int)(languageData.OffsetToData - resourceSection.VirtualAddress + resourceSection.PointerToRawData);
        var size = (int)languageData.Size1;
        
        return offset + size <= fileBytes.Length ? fileBytes[offset..(offset + size)] : null;
    }
    
    internal static bool IsPng(byte[] data) =>
        data.Length >= 4 && 
        data[0] == PngMagic0 
        && data[1] == PngMagic1 
        && data[2] == PngMagic2 
        && data[3] == PngMagic3;

    private static byte[] WrapInIco(byte[] iconData)
    {
        int width = iconData.Length >= BmpWidthOffset + 4 ? 
            BitConverter.ToInt32(iconData, BmpWidthOffset) : 0;
        int height = iconData.Length >= BmpHeightOffset + 4 ?
            BitConverter.ToInt32(iconData, BmpHeightOffset) / 2 : 0;
        int icoDataOffSet = IcoDirHeaderSize + IcoDirEntrySize;

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);
        
        writer.Write(IcoReserved);
        writer.Write(IcoTypeIcon);
        writer.Write((ushort)1);
        
        writer.Write((byte)(width > 255 ? 0 : width));         
        writer.Write((byte)(height > 255 ? 0 : height));         
        writer.Write((byte)0);                         
        writer.Write((byte)0);                         
        writer.Write((ushort)1);                       
        writer.Write((ushort)32);                      
        writer.Write((uint)iconData.Length);
        writer.Write((uint)icoDataOffSet);             

        writer.Write(iconData);
        writer.Flush();
        
        return ms.ToArray();

    }
}