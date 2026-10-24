// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;

namespace Restall.Application.Interfaces.Driven;

public interface IWebLinkLauncher
{
    /// <summary>
    /// Opens a URL with the operating system's default browser.
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.ProcessStartFailed"/>
    /// </para>
    /// </summary>
    Result OpenWebLink(Uri url);
}
