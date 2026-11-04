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
    bool HasActiveDanger);

/// <summary>Grupa pomieszczeń w <c>CollectionView</c> z <c>IsGrouped="True"</c>.</summary>
public sealed class RoomGroup(string name, IEnumerable<RoomSummary> rooms) : List<RoomSummary>(rooms)
{
    public string Name { get; } = name;
}
