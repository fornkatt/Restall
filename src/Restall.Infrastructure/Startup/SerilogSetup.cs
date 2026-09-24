// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Services;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;

namespace Restall.Infrastructure.Startup;

public static class SerilogSetup
{
    private const string LogFilename = "restall-.log";

    private const string OutputTemplate = "[{@t:HH:mm:ss.fff}] [{@l:u3}] [{ShortContext,-30}]{#if IsDefined(EventId)}" +
                                          " [{EventId.Id,4}]{#end} {@m}\n{@x}";

    private const string ShortContextExpression = "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)";

    public static void CreateBootstrapLogger()
    {
        var pathService = new PathService();
        var config = CreateBaseConfiguration().MinimumLevel.Information();
        AddLogFile(config.WriteTo, pathService);
        Log.Logger = config.CreateLogger();
    }

    public static void ReplaceBootstrapLogger(IPathService pathService, LoggingLevelSwitch levelSwitch)
    {
        Log.CloseAndFlush();
        Log.Logger = CreateBaseConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
            .WriteTo.Async(a => AddLogFile(a, pathService))
            .CreateLogger();
    }

    private static LoggerConfiguration CreateBaseConfiguration() =>
        new LoggerConfiguration().Enrich.WithComputed("ShortContext", ShortContextExpression);

    private static void AddLogFile(LoggerSinkConfiguration writeTo, IPathService pathService) =>
        writeTo.File(
            new ExpressionTemplate(OutputTemplate),
            Path.Combine(pathService.GetDefaultLogPath(), LogFilename),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 10);
}
