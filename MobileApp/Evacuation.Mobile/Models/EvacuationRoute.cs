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

/// <summary>Pojedynczy odcinek trasy — jedna instrukcja na liście kroków.</summary>
public sealed record RouteStep(int Ordinal, string Instruction);
