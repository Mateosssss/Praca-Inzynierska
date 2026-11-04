namespace Evacuation.Mobile.Models;

/// <summary>
/// Wynik działania Dijkstry po stronie backendu — trasa do najbliższego wyjścia ewakuacyjnego.
/// </summary>
public sealed record EvacuationRoute(
    string Headline,
    int TotalMeters,
    string Summary,
    IReadOnlyList<RouteStep> Steps,
    string? ComparisonNote = null)
{
    public string TotalMetersLabel => $"{TotalMeters} m";

    public bool HasComparison => ComparisonNote is not null;
}

/// <summary>
/// Pojedynczy przystanek trasy. Współrzędne to centroid pomieszczenia w pikselach rzutu —
/// z nich mapa rysuje łamaną trasy i na nie centruje widok przy krokowaniu.
/// </summary>
public sealed record RouteStep(
    int Ordinal,
    string Instruction,
    Guid RoomId = default,
    int RegionId = 0,
    int X = 0,
    int Y = 0);
