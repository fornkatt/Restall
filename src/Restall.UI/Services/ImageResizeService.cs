// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Media.Imaging;
using Restall.Application.Interfaces.Driven;
using System.IO;
using System.Threading.Tasks;

namespace Restall.UI.Services;

internal sealed class ImageResizeService : IImageResizeService
{
    public Task<byte[]> ReSizeImageToWidthAsync(byte[] imageBytes, int width)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var bitmap = Bitmap.DecodeToWidth(inputStream, width);
        using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, PngBitmapEncoderOptions.Default);
        return Task.FromResult(outputStream.ToArray());
    }
}
