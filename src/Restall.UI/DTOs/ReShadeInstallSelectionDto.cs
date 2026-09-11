// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.UI.DTOs;

public record ReShadeInstallSelectionDto(
    string Version,
    ReShade.Filename Filename,
    ReShade.FileExtension FileExtension);
