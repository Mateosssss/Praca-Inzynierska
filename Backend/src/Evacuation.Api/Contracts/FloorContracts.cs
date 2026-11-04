using Microsoft.AspNetCore.Mvc;

namespace Evacuation.Api.Contracts;

/// <summary>Wgranie rzutu. Przesyłane jako <c>multipart/form-data</c> razem z plikiem PNG.</summary>
public sealed class UploadFloorPlanRequest
{
    [FromForm]
    public IFormFile? Image { get; set; }

    /// <summary>Numer kondygnacji ze znakiem — parter to 0, piwnice ujemne.</summary>
    [FromForm]
    public int Level { get; set; }

    [FromForm]
    public string Label { get; set; } = string.Empty;

    /// <summary>Ile pikseli rzutu przypada na metr. Bez tego dystansów nie da się podać w metrach.</summary>
    [FromForm]
    public double PixelsPerMeter { get; set; }
}

/// <summary>
/// Wynik przetworzenia rzutu pokazywany administratorowi przed zatwierdzeniem —
/// odpowiednik ekranu „Wynik segmentacji” z makiety.
/// </summary>
public sealed record FloorPlanImportResponse(
    Guid Id,
    int Level,
    string Label,
    int WidthPx,
    int HeightPx,
    int RoomCount,
    int EdgeCount,
    int ExitCount,
    int StaircaseCount,
    bool GraphIsConnected,
    IReadOnlyList<int> RoomsWithoutExit,
    IReadOnlyList<PlanWarningResponse> Warnings);

public sealed record PlanWarningResponse(string Message, int X, int Y);

public sealed record RoomSummaryResponse(
    Guid Id,
    int RegionId,
    string Label,
    string Detail,
    double AreaSquareMeters,
    int CentroidX,
    int CentroidY,
    bool IsExit,
    bool IsStaircase,
    bool HasActiveDanger);
