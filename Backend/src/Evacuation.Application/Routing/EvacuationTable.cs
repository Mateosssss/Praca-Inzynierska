namespace Evacuation.Application.Routing;

/// <summary>
/// Tablica ewakuacyjna całej kondygnacji: dla każdego pomieszczenia odległość do najbliższego
/// wyjścia i pierwszy krok, który należy wykonać.
/// </summary>
/// <remarks>
/// Liczona raz na piętro jednym przebiegiem Dijkstry. Pytanie użytkownika „jestem tutaj,
/// dokąd mam iść” sprowadza się wtedy do odczytu z tablicy, a nie do przeszukiwania grafu —
/// co ma znaczenie, gdy podczas ewakuacji pyta jednocześnie wiele osób.
/// </remarks>
public sealed class EvacuationTable
{
    private readonly double[] _cost;
    private readonly double[] _length;
    private readonly int[] _nextRoom;

    internal EvacuationTable(double[] cost, double[] length, int[] nextRoom)
    {
        _cost = cost;
        _length = length;
        _nextRoom = nextRoom;
    }

    public int RoomCount => _cost.Length;

    /// <summary>Czy z tego pomieszczenia da się w ogóle dojść do jakiegokolwiek wyjścia.</summary>
    public bool CanReachExit(int roomId) => !double.IsPositiveInfinity(_cost[roomId]);

    /// <summary>
    /// Pomieszczenia, z których nie ma drogi do żadnego wyjścia. Niepusty wynik to usterka rzutu —
    /// sala odcięta od wszystkich wyjść nie powinna istnieć w poprawnie zaprojektowanym budynku.
    /// </summary>
    public IReadOnlyList<int> RoomsWithoutExit()
    {
        var odciete = new List<int>();

        for (var roomId = 0; roomId < _cost.Length; roomId++)
        {
            if (!CanReachExit(roomId))
            {
                odciete.Add(roomId);
            }
        }

        return odciete;
    }

    /// <summary>
    /// Trasa z podanego pomieszczenia do najbliższego wyjścia
    /// albo <c>null</c>, gdy żadne wyjście nie jest osiągalne.
    /// </summary>
    public EvacuationRoute? RouteFrom(int roomId)
    {
        if (!CanReachExit(roomId))
        {
            return null;
        }

        var path = new List<int> { roomId };
        var current = roomId;

        // _nextRoom wskazuje sąsiada bliższego wyjściu. Idąc tymi wskaźnikami,
        // dochodzimy do pomieszczenia z wyjściem, gdzie wskaźnik ma wartość -1.
        while (_nextRoom[current] != -1)
        {
            current = _nextRoom[current];
            path.Add(current);
        }

        return new EvacuationRoute(path, current, _length[roomId], _cost[roomId]);
    }
}
