namespace Evacuation.Mobile.Models;

/// <summary>
/// Zgłoszona strefa niebezpieczna wraz z metrykami przeliczenia trasy.
/// Strefa nie blokuje krawędzi grafu — podnosi jej koszt, więc przejście przez zagrożenie
/// pozostaje możliwe, jeśli nie ma innej drogi.
/// </summary>
public sealed record DangerZoneAlert(
    string Title,
    string Description,
    string Kind,
    TimeOnly ReportedAt,
    string CostMultiplier,
    string RecalculationStats)
{
    public string ReportedAtLabel =>
        $"zgłoszono {ReportedAt:HH:mm:ss} · trasa przeliczona automatycznie";
}

/// <summary>Odpowiedź na zgłoszenie zagrożenia: alarm plus trasa policzona od nowa.</summary>
public sealed record DangerAlertResult(DangerZoneAlert Alert, EvacuationRoute Route);
