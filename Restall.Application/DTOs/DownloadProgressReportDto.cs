namespace Restall.Application.DTOs;

public record DownloadProgressReportDto(
    string FileName,
    long BytesReceived,
    long? TotalBytes,
    int PercentComplete
    );