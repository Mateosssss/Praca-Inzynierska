namespace Evacuation.Application.Routing;

/// <summary>
/// Parametry wyznaczania trasy. Opisują sytuację w budynku w danej chwili,
/// a nie sam budynek — dlatego nie są częścią grafu zapisanego w bazie.
/// </summary>
public sealed record RoutingOptions
{
    /// <summary>
    /// Pomieszczenia zgłoszone jako niebezpieczne już po wgraniu rzutu — pożar, zadymienie,
    /// zablokowane przejście. Sumują się ze strefami narysowanymi na samym rzucie.
    /// </summary>
    public IReadOnlySet<int> ReportedDangerRoomIds { get; init; } = new HashSet<int>();

    /// <summary>
    /// Ile razy droższe jest przejście krawędzią dotykającą strefy zagrożenia.
    /// </summary>
    /// <remarks>
    /// Strefa podnosi koszt, a nie usuwa krawędzi. Dzięki temu, gdy zagrożenie odcina jedyną drogę,
    /// system poprowadzi przez nie zamiast odpowiedzieć „brak trasy” — w ewakuacji gorszy wynik
    /// jest lepszy niż jego brak. Przy mnożniku 25 trasa przez zagrożenie zostaje wybrana dopiero,
    /// gdy obejście jest ponad 25 razy dłuższe.
    /// </remarks>
    public double DangerMultiplier { get; init; } = 25.0;

    public static RoutingOptions Default { get; } = new();
}
