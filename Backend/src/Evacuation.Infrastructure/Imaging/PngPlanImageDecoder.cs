using Evacuation.Application.Imaging;
using SkiaSharp;

namespace Evacuation.Infrastructure.Imaging;

/// <summary>
/// Dekoder rzutów oparty na SkiaSharp.
/// </summary>
/// <remarks>
/// Biblioteka zewnętrzna odpowiada wyłącznie za rozpakowanie PNG do bitmapy — klasyfikacja
/// pikseli, segmentacja i budowa grafu są własną implementacją w warstwie <c>Application</c>.
///
/// Wybór padł na SkiaSharp, bo jest na licencji MIT i nie ma znanych podatności. ImageSharp,
/// naturalny kandydat, odpadł z dwóch powodów: wersja 3.2 blokuje budowanie w konfiguracji Release
/// bez wykupionego klucza Six Labors, a ostatnia wersja bez tej bramki ma cztery podatności
/// o wysokiej wadze — w komponencie parsującym pliki wgrywane przez użytkownika, czyli dokładnie
/// tam, gdzie przebiega granica zaufania.
/// </remarks>
public sealed class PngPlanImageDecoder : IPlanImageDecoder
{
    public Plan Decode(Stream image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using var codec = SKCodec.Create(image)
            ?? throw new InvalidPlanImageException("Wgranego pliku nie udało się rozpoznać jako obrazu.");

        if (codec.EncodedFormat != SKEncodedImageFormat.Png)
        {
            throw new InvalidPlanImageException(
                $"Rzut musi być plikiem PNG, a wgrany plik rozpoznano jako {codec.EncodedFormat}. "
                + "Formaty stratne zmieniają kolory pikseli i uniemożliwiają odczytanie kontraktu.");
        }

        var width = codec.Info.Width;
        var height = codec.Info.Height;

        if (width == 0 || height == 0)
        {
            throw new InvalidPlanImageException("Rzut jest pusty — obraz ma zerowy wymiar.");
        }

        // Wymuszamy jeden format pikseli niezależnie od tego, czy PNG był w palecie,
        // w odcieniach szarości czy w pełnym kolorze. Dalszy kod widzi zawsze RGBA po bajcie.
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        using var bitmap = new SKBitmap(info);

        var result = codec.GetPixels(info, bitmap.GetPixels());

        if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
        {
            throw new InvalidPlanImageException(
                $"Plik PNG jest uszkodzony i nie dało się go odczytać (kod: {result}).");
        }

        return ToPlan(bitmap, width, height);
    }

    private static Plan ToPlan(SKBitmap bitmap, int width, int height)
    {
        var plan = new Plan(width, height);
        var pixels = bitmap.GetPixelSpan();
        var rowBytes = bitmap.RowBytes;

        for (var y = 0; y < height; y++)
        {
            // Wiersz bitmapy bywa dopełniony do równej długości, więc o początku kolejnego
            // decyduje RowBytes, a nie szerokość razy rozmiar piksela.
            var row = pixels.Slice(y * rowBytes, width * 4);

            for (var x = 0; x < width; x++)
            {
                var offset = x * 4;

                plan[x, y] = PixelTranslator.FromRgb(row[offset], row[offset + 1], row[offset + 2]);
            }
        }

        return plan;
    }
}
