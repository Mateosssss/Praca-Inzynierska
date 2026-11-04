using Evacuation.Application.Imaging;
using SkiaSharp;

namespace Evacuation.Infrastructure.Tests;

/// <summary>
/// Buduje w pamięci prawdziwe pliki obrazów z rysunku ASCII — ten sam zapis plansz,
/// którego używają testy warstwy algorytmów. Dzięki temu w repozytorium nie leżą binarne
/// pliki testowe, a rysunek nadal widać w kodzie testu.
/// </summary>
internal static class PlanImage
{
    private static PixelKind FromChar(char symbol) => symbol switch
    {
        '.' => PixelKind.Floor,
        '#' => PixelKind.Wall,
        'E' => PixelKind.Exit,
        'D' => PixelKind.DangerZone,
        'S' => PixelKind.Stairs,
        'Y' => PixelKind.Start,
        '+' => PixelKind.Door,
        '?' => PixelKind.Unknown,
        _ => throw new ArgumentException($"Nieznany znak '{symbol}'."),
    };

    internal static MemoryStream Png(params string[] rows) =>
        Encode(rows, SKEncodedImageFormat.Png);

    internal static MemoryStream Jpeg(params string[] rows) =>
        Encode(rows, SKEncodedImageFormat.Jpeg);

    private static MemoryStream Encode(string[] rows, SKEncodedImageFormat format)
    {
        var info = new SKImageInfo(rows[0].Length, rows.Length, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        using var bitmap = new SKBitmap(info);

        for (var y = 0; y < rows.Length; y++)
        {
            for (var x = 0; x < rows[y].Length; x++)
            {
                var (r, g, b) = PixelTranslator.ToRgb(FromChar(rows[y][x]));

                bitmap.SetPixel(x, y, new SKColor(r, g, b));
            }
        }

        var stream = new MemoryStream();

        using (var data = bitmap.Encode(format, quality: 90))
        {
            data.SaveTo(stream);
        }

        stream.Position = 0;

        return stream;
    }
}
