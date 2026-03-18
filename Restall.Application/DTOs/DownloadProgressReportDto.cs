namespace Restall.Application.DTOs;

public record DownloadProgressReportDto(
    string Filename,
    long BytesReceived,
    long? TotalBytes,
    int PercentComplete
    );