using Evacuation.Application.Imaging;
using Evacuation.Application.Segmentation;

namespace Evacuation.Application.Graph;

/// <summary>
/// Zamienia wynik segmentacji w graf kondygnacji.
/// </summary>
/// <remarks>
/// Drzwi były podczas segmentacji barierą, więc nie należą do żadnego pomieszczenia.
/// Tutaj dostają swoją rolę: etykietyzujemy je tym samym sposobem, co pomieszczenia,
/// a potem dla każdego obszaru drzwi sprawdzamy, których pomieszczeń dotyka.
/// Dwa dotknięte pomieszczenia oznaczają krawędź grafu.
/// </remarks>
public static class GraphBuilder
{
    /// <summary>Lewo, prawo, góra, dół — tak samo jak przy segmentacji pomieszczeń.</summary>
    private static readonly (int Dx, int Dy)[] Neighbours =
    [
        (-1, 0),
        (1, 0),
        (0, -1),
        (0, 1),
    ];

    public static FloorGraph Build(Plan plan, SegmentationResult rooms)
    {
        var doorLabels = new int[plan.Width * plan.Height];
        Array.Fill(doorLabels, -1);

        // Para pomieszczeń → najtańsze znalezione przejście między nimi.
        // Gdy dwie sale łączy kilkoro drzwi, zostawiamy te, przez które jest bliżej.
        var edges = new Dictionary<(int Lower, int Upper), double>();
        var warnings = new List<PlanWarning>();
        var nextDoorId = 0;

        for (var y = 0; y < plan.Height; y++)
        {
            for (var x = 0; x < plan.Width; x++)
            {
                if (plan[x, y] != PixelKind.Door)
                {
                    continue;
                }

                if (doorLabels[y * plan.Width + x] != -1)
                {
                    continue;
                }

                var door = FillDoor(plan, rooms, doorLabels, x, y, nextDoorId++);
                Classify(door, rooms, edges, warnings);
            }
        }

        var edgeList = edges
            .Select(pair => new RoomEdge(pair.Key.Lower, pair.Key.Upper, pair.Value))
            .OrderBy(edge => edge.FromRoomId)
            .ThenBy(edge => edge.ToRoomId)
            .ToList();

        return new FloorGraph(rooms.Regions, edgeList, warnings);
    }

    private static void Classify(
        Door door,
        SegmentationResult rooms,
        Dictionary<(int Lower, int Upper), double> edges,
        List<PlanWarning> warnings)
    {
        switch (door.TouchingRooms.Count)
        {
            case 0:
                warnings.Add(new PlanWarning(
                    "Drzwi nie przylegają do żadnego pomieszczenia — prawdopodobnie otacza je ściana.",
                    door.CentroidX,
                    door.CentroidY));
                break;

            case 1:
                // Drzwi na zewnątrz budynku. Poprawne i częste przy wyjściach ewakuacyjnych,
                // ale nie łączą dwóch pomieszczeń, więc nie tworzą krawędzi.
                break;

            case 2:
                AddEdge(door, rooms, edges);
                break;

            default:
                warnings.Add(new PlanWarning(
                    $"Drzwi stykają się z {door.TouchingRooms.Count} pomieszczeniami. "
                    + "Przejście łączy dokładnie dwa — sprawdź, czy w tym miejscu nie brakuje ściany.",
                    door.CentroidX,
                    door.CentroidY));
                break;
        }
    }

    private static void AddEdge(
        Door door,
        SegmentationResult rooms,
        Dictionary<(int Lower, int Upper), double> edges)
    {
        var ids = door.TouchingRooms.Order().ToArray();
        var first = rooms.Regions[ids[0]];
        var second = rooms.Regions[ids[1]];

        var weight =
            Distance(first.CentroidX, first.CentroidY, door.CentroidX, door.CentroidY)
            + Distance(door.CentroidX, door.CentroidY, second.CentroidX, second.CentroidY);

        var key = (ids[0], ids[1]);

        if (!edges.TryGetValue(key, out var current) || weight < current)
        {
            edges[key] = weight;
        }
    }

    /// <summary>
    /// Rozlewa etykietę po spójnym obszarze drzwi, licząc przy okazji jego środek
    /// i zbierając numery pomieszczeń, których dotyka.
    /// </summary>
    private static Door FillDoor(
        Plan plan,
        SegmentationResult rooms,
        int[] doorLabels,
        int startX,
        int startY,
        int id)
    {
        var pixelCount = 0;
        long sumX = 0;
        long sumY = 0;
        var touching = new HashSet<int>();

        var stack = new Stack<(int X, int Y)>();

        doorLabels[startY * plan.Width + startX] = id;
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            pixelCount++;
            sumX += x;
            sumY += y;

            foreach (var (dx, dy) in Neighbours)
            {
                var nx = x + dx;
                var ny = y + dy;

                // RegionAt zwraca -1 również dla współrzędnych spoza planu,
                // więc pytanie o sąsiada przy krawędzi jest bezpieczne.
                var room = rooms.RegionAt(nx, ny);

                if (room >= 0)
                {
                    touching.Add(room);
                    continue;
                }

                if (nx < 0 || nx >= plan.Width || ny < 0 || ny >= plan.Height)
                {
                    continue;
                }

                if (plan[nx, ny] != PixelKind.Door)
                {
                    continue;
                }

                var neighbourIndex = ny * plan.Width + nx;

                if (doorLabels[neighbourIndex] != -1)
                {
                    continue;
                }

                doorLabels[neighbourIndex] = id;
                stack.Push((nx, ny));
            }
        }

        return new Door((int)(sumX / pixelCount), (int)(sumY / pixelCount), touching);
    }

    private static double Distance(int x1, int y1, int x2, int y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;

        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private readonly record struct Door(int CentroidX, int CentroidY, HashSet<int> TouchingRooms);
}
