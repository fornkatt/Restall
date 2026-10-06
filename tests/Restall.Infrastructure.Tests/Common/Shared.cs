// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.Tests.Common;

public static class Shared
{
    public static bool IsLinux => OperatingSystem.IsLinux();

    public static bool IsWindows => OperatingSystem.IsWindows();
}
