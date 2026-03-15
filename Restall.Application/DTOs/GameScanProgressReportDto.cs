namespace Restall.Application.DTOs;

public record GameScanProgressReportDto(
    string CompletedPlatform,
    int ScannersCompleted,
    int TotalScanners,
    bool Success,
    string? ErrorMessage = null
    );