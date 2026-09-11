// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.Interfaces.Driven;

public interface IImageResizeService
{
    Task<byte[]> ReSizeImageToWidthAsync(byte[] imageBytes, int width);
}
