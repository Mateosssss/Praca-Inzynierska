namespace Evacuation.Mobile.Models;

/// <summary>
/// Pomieszczenie wykryte przez segmentację rzutu. <paramref name="RegionId"/> to numer regionu
/// nadany przez flood fill — ta sama wartość, która jest wierzchołkiem w grafie.
/// </summary>
public sealed record RoomSummary(
    Guid Id,
    int RegionId,
    string Label,
    string Group,
    string Detail,
    bool IsExit,
    bool IsStaircase,
    bool HasActiveDanger,
    int CentroidX = 0,
    int CentroidY = 0)
{
    /// <summary>
    /// Etykieta w dopełniaczu, do zdań typu „Wyznacz trasę z sali 118”.
    /// Obsługiwane są wzorce, które faktycznie występują; etykieta spoza nich zostaje
    /// bez zmian, bo lepiej zabrzmieć sztywno niż wyprodukować kalkę językową.
    /// </summary>
    public string LabelGenitive => System.Text.RegularExpressions.Regex.Match(Label, @"^(\S+)\s+(.+?)(?:\s+—.*)?$") switch
    {
        { Success: true } m => m.Groups[1].Value switch
        {
            "Sala" => $"sali {m.Groups[2].Value}",
            "Pomieszczenie" => $"pomieszczenia {m.Groups[2].Value}",
            "Korytarz" => $"korytarza {m.Groups[2].Value}",
            "Klatka" => $"klatki {m.Groups[2].Value}",
            _ => Label,
        },
        _ => Label,
    };
}

/// <summary>Grupa pomieszczeń w <c>CollectionView</c> z <c>IsGrouped="True"</c>.</summary>
public sealed class RoomGroup(string name, IEnumerable<RoomSummary> rooms) : List<RoomSummary>(rooms)
{
    public string Name { get; } = name;
}
