// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.CommonTests;

public class RenoDXTextsTest
{
    [Fact]
    public void GetStatusText_EveryStatus_ReturnsText()
    {
        Assert.All(Enum.GetValues<RenoDXModStatus>(),
            status => Assert.False(string.IsNullOrWhiteSpace(RenoDXTexts.GetStatusText(status))));
    }

    [Fact]
    public void GetUnrealMethodText_EveryMethod_ReturnsText()
    {
        Assert.All(Enum.GetValues<RenoDXUnrealGenericMod.UnrealModMethod>(),
            method => Assert.False(string.IsNullOrWhiteSpace(RenoDXTexts.GetUnrealMethodText(method))));
    }

    [Fact]
    public void GetNoticeText_EveryNotice_ReturnsText()
    {
        Assert.All(Enum.GetValues<RenoDXAvailability.Notice>(),
            notice => Assert.False(string.IsNullOrWhiteSpace(RenoDXTexts.GetNoticeText(notice))));
    }
}
