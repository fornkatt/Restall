// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;

namespace Restall.Infrastructure.Tests.Fakes;

public sealed class FakeDownloadProgress : IProgress<DownloadProgressReport>
{
    public List<DownloadProgressReport> Reports { get; } = [];
    public void Report(DownloadProgressReport value) => Reports.Add(value);
}
