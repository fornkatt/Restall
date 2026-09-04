using System.IO;
using Avalonia.Media.Imaging;
using Restall.Application.Interfaces.Driven;

namespace Restall.UI.Services;

//TODO: REMOVE THIS AND FOCUS ON IMAGERESIZESERVICE FOR REFACTOR, THEY SERVE SAME PURPOSE AND HAVE TWO SCRIPTS ARE UNNECESSARY
internal sealed class IconConverterService : IIconConverterService
{
    public byte[] IcoToPng(byte[] icoBytes, int width)
    {
        using var icoStream = new MemoryStream(icoBytes);
        using var bitmap = Bitmap.DecodeToWidth(icoStream,width);
        using var pngStream = new MemoryStream();
        bitmap.Save(pngStream, PngBitmapEncoderOptions.Default);
        return pngStream.ToArray();
    }
}