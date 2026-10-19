using Evacuation.Application.Imaging;

namespace Evacuation.Application.Segmentation;

/// <summary>
/// Dzieli rzut na pomieszczenia metodą etykietyzacji spójnych obszarów.
/// Ściany i drzwi są barierą — dzięki temu sale nie zlewają się w jeden obszar,
/// a informacja o połączeniu między nimi pochodzi dopiero z pikseli drzwi.
/// </summary>
public static class RoomSegmenter
{
    /// <summary>
    /// Przesunięcia do czterech sąsiadów: lewo, prawo, góra, dół.
    /// Sąsiedztwo czterokierunkowe, nie ośmiokierunkowe — przy ośmiu kierunkach
    /// wypełnianie przeciekałoby po skosie przez narożniki ścian grubych na piksel.
    /// </summary>
    private static readonly (int Dx, int Dy)[] Neighbours =
    [
        (-1, 0),
        (1, 0),
        (0, -1),
        (0, 1),
    ];

    public static SegmentationResult Segment(Plan plan)
    {
        var labels = new int[plan.Width * plan.Height];
        Array.Fill(labels, -1);

        var regions = new List<Region>();

        // Przeczesywanie: od lewej do prawej, wiersz po wierszu. Zatrzymujemy się
        // na pierwszym pikselu pomieszczenia bez etykiety — to nowy, nieodwiedzony region.
        for (var y = 0; y < plan.Height; y++)
        {
            for (var x = 0; x < plan.Width; x++)
            {
                if (!IsRoomPixel(plan[x, y]))
                {
                    continue;
                }

                if (labels[y * plan.Width + x] != -1)
                {
                    continue;
                }

                // regions.Count to jednocześnie numer kolejnego regionu,
                // dzięki czemu zachodzi Regions[i].Id == i.
                regions.Add(Fill(plan, labels, x, y, regions.Count));
            }
        }

        return new SegmentationResult(plan.Width, plan.Height, regions, labels);
    }

    /// <summary>
    /// Czy po takim pikselu da się chodzić i czy należy on do pomieszczenia.
    /// Drzwi dają <c>false</c> celowo: na etapie segmentacji są barierą, nie przejściem.
    /// </summary>
    private static bool IsRoomPixel(PixelKind kind) => kind switch
    {
        PixelKind.Floor or PixelKind.Exit or PixelKind.DangerZone
            or PixelKind.Stairs or PixelKind.Start => true,
        _ => false,
    };

    /// <summary>
    /// Rozlewa etykietę <paramref name="id"/> po całym pomieszczeniu zawierającym
    /// piksel (<paramref name="startX"/>, <paramref name="startY"/>) i zwraca jego opis.
    /// </summary>
    private static Region Fill(Plan plan, int[] labels, int startX, int startY, int id)
    {
        var pixelCount = 0;

        // Sumy jako long: przy rzucie o dwóch milionach pikseli i szerokości rzędu
        // tysięcy suma współrzędnych przekracza zakres int i centroid wyszedłby ujemny.
        long sumX = 0;
        long sumY = 0;

        var hasExit = false;
        var hasStairs = false;
        var hasDanger = false;
        var hasStart = false;

        // Stos pełni rolę listy „do odwiedzenia”. Iteracyjnie, nie rekurencyjnie —
        // rekurencja na pomieszczeniu liczącym miliony pikseli przepełniłaby stos wątku.
        var stack = new Stack<(int X, int Y)>();

        labels[startY * plan.Width + startX] = id;
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            // Statystyki zbieramy przy zdejmowaniu. Każdy piksel trafia na stos
            // dokładnie raz, bo etykietę nadajemy w chwili wrzucania — więc zostanie
            // też zdjęty dokładnie raz i nic nie policzy się podwójnie.
            pixelCount++;
            sumX += x;
            sumY += y;

            switch (plan[x, y])
            {
                case PixelKind.Exit:
                    hasExit = true;
                    break;
                case PixelKind.Stairs:
                    hasStairs = true;
                    break;
                case PixelKind.DangerZone:
                    hasDanger = true;
                    break;
                case PixelKind.Start:
                    hasStart = true;
                    break;
            }

            foreach (var (dx, dy) in Neighbours)
            {
                var nx = x + dx;
                var ny = y + dy;

                // Granice planu sprawdzamy sami: indekser Plan ich nie pilnuje
                // i przy x poza zakresem po cichu zwróciłby piksel z sąsiedniego wiersza.
                if (nx < 0 || nx >= plan.Width || ny < 0 || ny >= plan.Height)
                {
                    continue;
                }

                if (!IsRoomPixel(plan[nx, ny]))
                {
                    continue;
                }

                var neighbourIndex = ny * plan.Width + nx;

                if (labels[neighbourIndex] != -1)
                {
                    continue;
                }

                // Etykieta nadawana przy wrzucaniu, nie przy zdejmowaniu — inaczej ten sam
                // piksel trafiłby na stos z każdego sąsiada osobno i stos by spuchł.
                labels[neighbourIndex] = id;
                stack.Push((nx, ny));
            }
        }

        return new Region(
            id,
            pixelCount,
            (int)(sumX / pixelCount),
            (int)(sumY / pixelCount),
            hasExit,
            hasStairs,
            hasDanger,
            hasStart);
    }
}
