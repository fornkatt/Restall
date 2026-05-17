using System.IO;
using Avalonia.Media.Imaging;
using Restall.Application.Interfaces.Driven;

namespace Restall.UI.Services;

public sealed class IconConverterService : IIconConverterService
{
    public byte[] IcoToPng(byte[] icoBytes, int width)
    {
        using var icoStream = new MemoryStream(icoBytes);
        using var bitmap = Bitmap.DecodeToWidth(icoStream,width);
        using var pngStream = new MemoryStream();
        bitmap.Save(pngStream);
        return pngStream.ToArray();
    }
}