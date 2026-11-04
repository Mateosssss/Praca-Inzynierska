using Evacuation.Application.Graph;

namespace Evacuation.Application.Routing;

/// <summary>
/// Wyznacza trasy ewakuacji algorytmem Dijkstry.
/// </summary>
/// <remarks>
/// Zamiast liczyć trasę osobno do każdego wyjścia, przebieg startuje jednocześnie ze wszystkich
/// pomieszczeń mających wyjście — jest to równoważne dołożeniu do grafu wirtualnego wierzchołka
/// połączonego z każdym wyjściem krawędzią o wadze zero. Graf jest nieskierowany, więc jeden taki
/// przebieg daje od razu dla każdego pomieszczenia odległość do najbliższego wyjścia oraz kierunek,
/// w którym należy się z niego poruszyć.
/// </remarks>
public static class EvacuationRouter
{
    public static EvacuationTable BuildTable(FloorGraph graph, RoutingOptions? options = null)
    {
        options ??= RoutingOptions.Default;

        var roomCount = graph.Rooms.Count;
        var danger = CollectDangerRooms(graph, options);
        var adjacency = BuildAdjacency(graph, roomCount);

        // cost   — odległość z karami za strefy zagrożenia; po niej optymalizujemy
        // length — rzeczywista długość tej samej trasy, bez kar; ją pokazujemy użytkownikowi
        var cost = new double[roomCount];
        var length = new double[roomCount];
        var nextRoom = new int[roomCount];
        var settled = new bool[roomCount];

        Array.Fill(cost, double.PositiveInfinity);
        Array.Fill(length, double.PositiveInfinity);
        Array.Fill(nextRoom, -1);

        var queue = new PriorityQueue<int, double>();

        // Start jednocześnie ze wszystkich wyjść — to jest ów wirtualny wierzchołek.
        foreach (var room in graph.Rooms)
        {
            if (!room.HasExit)
            {
                continue;
            }

            cost[room.Id] = 0;
            length[room.Id] = 0;
            queue.Enqueue(room.Id, 0);
        }

        while (queue.TryDequeue(out var roomId, out _))
        {
            // PriorityQueue nie umie obniżać priorytetu, więc ten sam wierzchołek może trafić
            // do kolejki kilka razy. Pierwsze zdjęcie jest tym właściwym, kolejne pomijamy.
            if (settled[roomId])
            {
                continue;
            }

            settled[roomId] = true;

            foreach (var (neighbourId, weight) in adjacency[roomId])
            {
                if (settled[neighbourId])
                {
                    continue;
                }

                var multiplier = danger.Contains(roomId) || danger.Contains(neighbourId)
                    ? options.DangerMultiplier
                    : 1.0;

                var candidateCost = cost[roomId] + (weight * multiplier);

                if (candidateCost >= cost[neighbourId])
                {
                    continue;
                }

                cost[neighbourId] = candidateCost;
                length[neighbourId] = length[roomId] + weight;

                // Idziemy od wyjść w głąb budynku, więc dla sąsiada krokiem przybliżającym
                // do wyjścia jest pomieszczenie, z którego właśnie przyszliśmy.
                nextRoom[neighbourId] = roomId;

                queue.Enqueue(neighbourId, candidateCost);
            }
        }

        return new EvacuationTable(cost, length, nextRoom);
    }

    /// <summary>Trasa z jednego pomieszczenia. Wygodne, gdy pytanie jest pojedyncze.</summary>
    public static EvacuationRoute? FindRoute(FloorGraph graph, int startRoomId, RoutingOptions? options = null) =>
        BuildTable(graph, options).RouteFrom(startRoomId);

    /// <summary>
    /// Strefy narysowane na rzucie sumują się ze zgłoszonymi później.
    /// Czerwony obszar na planie oznacza zagrożenie trwałe, zgłoszenie — chwilowe.
    /// </summary>
    private static HashSet<int> CollectDangerRooms(FloorGraph graph, RoutingOptions options)
    {
        var danger = new HashSet<int>(options.ReportedDangerRoomIds);

        foreach (var room in graph.Rooms)
        {
            if (room.HasDanger)
            {
                danger.Add(room.Id);
            }
        }

        return danger;
    }

    private static List<(int RoomId, double Weight)>[] BuildAdjacency(FloorGraph graph, int roomCount)
    {
        var adjacency = new List<(int RoomId, double Weight)>[roomCount];

        for (var i = 0; i < roomCount; i++)
        {
            adjacency[i] = [];
        }

        // Krawędzie są nieskierowane — ewakuować można się w obie strony.
        foreach (var edge in graph.Edges)
        {
            adjacency[edge.FromRoomId].Add((edge.ToRoomId, edge.WeightPixels));
            adjacency[edge.ToRoomId].Add((edge.FromRoomId, edge.WeightPixels));
        }

        return adjacency;
    }
}
