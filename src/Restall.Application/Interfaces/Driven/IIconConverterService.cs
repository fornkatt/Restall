namespace Restall.Application.Interfaces.Driven;

public interface IIconConverterService
{
    byte[] IcoToPng(byte[] icoBytes, int width);
}