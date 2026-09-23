// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.Interfaces.Driven;

public interface IIconConverterService
{
    byte[] IcoToPng(byte[] icoBytes, int width);
}
