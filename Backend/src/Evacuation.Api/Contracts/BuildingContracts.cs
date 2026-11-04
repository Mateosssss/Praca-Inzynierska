namespace Evacuation.Api.Contracts;

public sealed record CreateBuildingRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude);

/// <summary>Budynek na liście „w pobliżu”, wraz z odległością od pytającego.</summary>
public sealed record NearbyBuildingResponse(
    Guid Id,
    string Name,
    string Address,
    int DistanceMeters,
    int FloorCount,
    string? AlarmNote);

/// <summary>Kondygnacja na ekranie wyboru piętra.</summary>
public sealed record FloorSummaryResponse(
    Guid Id,
    int Level,
    string Label,
    int RoomCount,
    int StaircaseCount,
    int ExitCount,
    int DangerZoneCount,
    bool HasPlan);
