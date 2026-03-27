namespace Restall.Application.DTOs;

public record DownloadProgressReportDto(
    string Filename,
    int PercentComplete
    );