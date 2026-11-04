namespace Evacuation.Api.Contracts;

/// <summary>Trasa ewakuacji zwracana aplikacji mobilnej.</summary>
/// <param name="TotalMeters">
/// Rzeczywista długość po przeliczeniu przez skalę rzutu — bez kar za strefy zagrożenia.
/// </param>
/// <param name="PassesThroughDanger">
/// Czy trasa prowadzi przez strefę zagrożenia. Prawda oznacza, że bezpieczniejszego obejścia
/// nie było — system prowadzi przez zagrożenie zamiast odmówić wyznaczenia trasy.
/// </param>
public sealed record RouteResponse(
    Guid ExitRoomId,
    string ExitLabel,
    double TotalMeters,
    int SegmentCount,
    bool PassesThroughDanger,
    IReadOnlyList<RouteStepResponse> Steps);

public sealed record RouteStepResponse(
    int Ordinal,
    Guid RoomId,
    int RegionId,
    string Label,
    int CentroidX,
    int CentroidY);

public sealed record ReportDangerRequest(string Kind);

public sealed record DangerZoneResponse(
    Guid Id,
    Guid RoomId,
    string Kind,
    DateTimeOffset ReportedAt,
    string Status);
