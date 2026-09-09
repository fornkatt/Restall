// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

﻿using Restall.Domain.Entities;

namespace Restall.Application.UseCases.Requests;

public record InstallReShadeRequest(
    Game Game,
    ReShade.Branch Branch,
    ReShade.Architecture Arch,
    string Version,
    string SelectedFilename
    );