namespace Evacuation.Domain;

/// <summary>
/// Zagrożenie zgłoszone w konkretnym pomieszczeniu już po wgraniu rzutu — pożar, zadymienie,
/// zablokowane przejście.
/// </summary>
/// <remarks>
/// Zgłoszenie jest chwilowe i nie zmienia grafu zapisanego w bazie. Graf opisuje budynek,
/// a zagrożenia nakładane są dopiero przy liczeniu trasy, jako mnożnik kosztu krawędzi.
/// Strefa podnosi koszt, a nie usuwa przejścia — gdy zagrożenie odcina jedyną drogę, system
/// poprowadzi przez nie zamiast odpowiedzieć „brak trasy”.
/// </remarks>
public class DangerZone
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    /// <summary>Rodzaj zagrożenia pokazywany użytkownikowi, np. „pożar”.</summary>
    public string Kind { get; set; } = string.Empty;

    public DangerZoneStatus Status { get; set; }

    /// <summary>Kiedy zagrożenie odwołano. <c>null</c>, dopóki jest aktywne.</summary>
    public DateTimeOffset? ClearedAt { get; set; }
}

/// <summary>Stan zgłoszenia zagrożenia.</summary>
public enum DangerZoneStatus
{
    /// <summary>Trwa — podnosi koszt krawędzi przy wyznaczaniu tras.</summary>
    Active = 0,

    /// <summary>Odwołane — nie wpływa już na trasy, ale zostaje w historii.</summary>
    Cleared = 1,
}
