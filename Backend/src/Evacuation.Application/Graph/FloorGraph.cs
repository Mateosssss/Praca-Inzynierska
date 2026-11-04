using Evacuation.Application.Segmentation;

namespace Evacuation.Application.Graph;

/// <summary>
/// Graf jednej kondygnacji: pomieszczenia jako wierzchołki, drzwi jako krawędzie.
/// To na nim będzie pracował Dijkstra i to on trafi do bazy jako <c>Room</c> i <c>RoomEdge</c>.
/// </summary>
public sealed class FloorGraph
{
    public FloorGraph(
        IReadOnlyList<Region> rooms,
        IReadOnlyList<RoomEdge> edges,
        IReadOnlyList<PlanWarning> warnings)
    {
        Rooms = rooms;
        Edges = edges;
        Warnings = warnings;
    }

    /// <summary>Wierzchołki. Zachodzi <c>Rooms[i].Id == i</c>.</summary>
    public IReadOnlyList<Region> Rooms { get; }

    /// <summary>Krawędzie wynikające z drzwi. Dla każdej pary pomieszczeń co najwyżej jedna.</summary>
    public IReadOnlyList<RoomEdge> Edges { get; }

    /// <summary>Usterki rzutu wykryte przy budowaniu grafu. Pusta lista oznacza rzut poprawny.</summary>
    public IReadOnlyList<PlanWarning> Warnings { get; }
}
