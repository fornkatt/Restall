// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Services;

namespace Restall.Infrastructure.Tests.Services;

public class RenoDXModSourceServiceTests
{
    private const string ValidGameModEntry =
        """
        {
           "name": "007 First Light",
           "status": "Done",
           "author": "Musa",
           "snapshotUrl": "https://github.com/mqhaji/renodx/releases/download/snapshot/renodx-007firstlight.addon64",
           "snapshotUrl32": null,
           "nexusUrl": "https://www.nexusmods.com/007firstlight/mods/37",
           "discordUrl": null,
           "discussionUrl": null,
           "notes": null
        }
        """;
}
