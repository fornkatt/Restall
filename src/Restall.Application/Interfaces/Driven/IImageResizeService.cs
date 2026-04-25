namespace Restall.Application.Interfaces.Driven;

public interface IImageResizeService
{
    Task<byte[]> ReSizeImageToWidthAsync(byte[] imageBytes, int width);
}