namespace Evacuation.Mobile.Models;

/// <summary>
/// Budynek znaleziony po współrzędnych GPS — pozycja listy na ekranie „Budynki w pobliżu”.
/// </summary>
public sealed record BuildingSummary(
    Guid Id,
    string Name,
    string Address,
    int DistanceMeters,
    int FloorCount,
    string? AlarmNote)
{
    public bool HasActiveAlarm => AlarmNote is not null;

    public string DistanceLabel => $"{DistanceMeters} m";

    /// <summary>Polska odmiana: 1 piętro, 2–4 piętra, 5+ pięter.</summary>
    public string FloorCountLabel => FloorCount switch
    {
        1 => "1 piętro",
        >= 2 and <= 4 => $"{FloorCount} piętra",
        _ => $"{FloorCount} pięter",
    };
}
