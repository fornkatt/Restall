// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.Tests.HeroicTests.Common;

public static class SharedHeroic
{
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsWindows => OperatingSystem.IsWindows();

    public static string ReadHeroicBlock(params string[] entries) =>
        "{" + string.Join(",", entries) + "}";
}
