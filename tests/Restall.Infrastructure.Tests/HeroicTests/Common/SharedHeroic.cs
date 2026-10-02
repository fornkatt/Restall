// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.Tests.HeroicTests.Common;

public static class SharedHeroic
{
    public static string ReadHeroicBlock(params string[] entries) =>
        "{" + string.Join(",", entries) + "}";

    public static string ReadHeroicBlockArray(string key, params string[] entries) =>
        ReadHeroicBlock($$"""{{key}}: [ {{string.Join(", ", entries)}} ]""");

}

//"""{ "installed": [ { "appName": "1207659037", "install_path": "C:\\Games\\Heroic\\Alan Wake" } ] }"""
