namespace Evacuation.Application.Routing;

/// <summary>
/// Trasa ewakuacji z jednego pomieszczenia do najbliższego wyjścia.
/// </summary>
/// <param name="RoomIds">
/// Kolejne pomieszczenia od startu do wyjścia włącznie. Pierwszy element to pomieszczenie startowe,
/// ostatni to <paramref name="ExitRoomId"/>. Dla pomieszczenia, które samo ma wyjście, lista ma
/// jeden element.
/// </param>
/// <param name="LengthPixels">
/// Rzeczywista długość trasy w pikselach — suma wag krawędzi, bez kar za strefy zagrożenia.
/// To ta wartość trafia na ekran użytkownika po przeliczeniu na metry.
/// </param>
/// <param name="CostPixels">
/// Koszt, według którego trasa została wybrana. Przy braku stref zagrożenia równy
/// <paramref name="LengthPixels"/>; gdy trasa przechodzi przez zagrożenie, jest od niej większy.
/// Rozbieżność między tymi dwiema wartościami mówi, ile system „dopłacił” za bezpieczeństwo.
/// </param>
public sealed record EvacuationRoute(
    IReadOnlyList<int> RoomIds,
    int ExitRoomId,
    double LengthPixels,
    double CostPixels)
{
    /// <summary>Liczba przejść przez drzwi — o jeden mniej niż odwiedzonych pomieszczeń.</summary>
    public int SegmentCount => RoomIds.Count - 1;

    /// <summary>Czy użytkownik już stoi w pomieszczeniu z wyjściem.</summary>
    public bool AlreadyAtExit => RoomIds.Count == 1;

    /// <summary>Czy trasa prowadzi przez strefę zagrożenia.</summary>
    public bool PassesThroughDanger => CostPixels > LengthPixels;
}
