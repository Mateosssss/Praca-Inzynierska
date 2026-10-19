using Evacuation.Application.Imaging;

namespace Evacuation.Application.Tests;

/// <summary>
/// Buduje <see cref="Plan"/> z rysunku ASCII. Dzięki temu plansze testowe stoją
/// bezpośrednio w kodzie testu i widać ich kształt wzrokiem, zamiast trzymać
/// pliki PNG w repozytorium i oglądać je w podglądzie obrazów, gdy test padnie.
/// </summary>
public static class PlanBuilder
{
    /// <summary>
    /// Legenda znaków:
    /// <c>.</c> podłoga, <c>#</c> ściana, <c>E</c> wyjście, <c>D</c> strefa niebezpieczna,
    /// <c>S</c> schody, <c>Y</c> punkt startowy, <c>+</c> drzwi, <c>?</c> kolor spoza kontraktu.
    /// </summary>
    public static Plan FromAscii(params string[] rows)
    {
        if (rows is null || rows.Length == 0)
        {
            throw new ArgumentException("Rysunek musi mieć co najmniej jeden wiersz.", nameof(rows));
        }

        var height = rows.Length;
        var width = rows[0].Length;

        if (width == 0)
        {
            throw new ArgumentException("Wiersze rysunku nie mogą być puste.", nameof(rows));
        }

        // Prostokątność sprawdzamy z góry: nierówny rysunek to zawsze literówka w teście,
        // a bez tej kontroli objawiłaby się dopiero jako dziwny wynik segmentacji.
        for (var y = 1; y < height; y++)
        {
            if (rows[y].Length != width)
            {
                throw new ArgumentException(
                    $"Wiersz {y} ma długość {rows[y].Length}, a wiersz 0 ma {width}. " +
                    "Wszystkie wiersze rysunku muszą być równej długości.",
                    nameof(rows));
            }
        }

        var plan = new Plan(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Uwaga na kolejność: w rysunku czytamy [wiersz][kolumna], czyli rows[y][x],
                // a w planie zapisujemy [kolumna, wiersz], czyli plan[x, y].
                plan[x, y] = FromChar(rows[y][x], x, y);
            }
        }

        return plan;
    }

    private static PixelKind FromChar(char symbol, int x, int y) => symbol switch
    {
        '.' => PixelKind.Floor,
        '#' => PixelKind.Wall,
        'E' => PixelKind.Exit,
        'D' => PixelKind.DangerZone,
        'S' => PixelKind.Stairs,
        'Y' => PixelKind.Start,
        '+' => PixelKind.Door,
        '?' => PixelKind.Unknown,
        _ => throw new ArgumentException($"Nieznany znak '{symbol}' na pozycji ({x}, {y})."),
    };
}
