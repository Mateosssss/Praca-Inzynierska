namespace Evacuation.Mobile.Services;

/// <summary>
/// Odpowiedzi Evacuation.Api w postaci, w jakiej przychodzą po drucie.
/// Celowo oddzielone od modeli ekranów: zmiana kształtu odpowiedzi nie powinna
/// pociągać za sobą zmian w widokach.
/// </summary>
internal sealed record NearbyBuildingDto(
    Guid Id,
    string Name,
    string Address,
    int DistanceMeters,
    int FloorCount,
    string? AlarmNote);

internal sealed record FloorSummaryDto(
    Guid Id,
    int Level,
    string Label,
    int RoomCount,
    int StaircaseCount,
    int ExitCount,
    int DangerZoneCount,
    bool HasPlan);

internal sealed record RoomSummaryDto(
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

internal sealed record RouteDto(
    Guid ExitRoomId,
    string ExitLabel,
    double TotalMeters,
    int SegmentCount,
    bool PassesThroughDanger,
    IReadOnlyList<RouteStepDto> Steps);

internal sealed record RouteStepDto(
    int Ordinal,
    Guid RoomId,
    int RegionId,
    string Label,
    int CentroidX,
    int CentroidY);

internal sealed record DangerZoneDto(
    Guid Id,
    Guid RoomId,
    string Kind,
    DateTimeOffset ReportedAt,
    string Status);

internal sealed record RoomAtDto(Guid? RoomId, int? RegionId);
