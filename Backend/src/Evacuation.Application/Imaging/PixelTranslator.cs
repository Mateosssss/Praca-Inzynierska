namespace Evacuation.Application.Imaging;

/// <summary>
/// Zamienia kolor piksela na jego znaczenie i z powrotem. Jedyne miejsce w całym projekcie,
/// w którym zapisany jest kontrakt kolorów.
/// </summary>
/// <remarks>
/// Porównanie jest dokładne, bez tolerancji i bez szukania najbliższego koloru. Kontrakt używa
/// narożników sześcianu RGB, więc wartości dzieli maksymalny możliwy dystans — a rzuty przyjmujemy
/// wyłącznie w PNG, który jest bezstratny. Przy JPEG biel zapisałaby się jako <c>#FEFEFE</c>
/// i trzeba by progować, czego ten projekt unika z założenia.
/// </remarks>
public static class PixelTranslator
{
    /// <summary>Kolor z rzutu → znaczenie. Kolor spoza kontraktu daje <see cref="PixelKind.Unknown"/>.</summary>
    public static PixelKind FromRgb(byte r, byte g, byte b) => (r, g, b) switch
    {
        (255, 255, 255) => PixelKind.Floor,
        (0, 0, 0) => PixelKind.Wall,
        (0, 255, 0) => PixelKind.Exit,
        (255, 0, 0) => PixelKind.DangerZone,
        (0, 0, 255) => PixelKind.Stairs,
        (255, 255, 0) => PixelKind.Start,
        (255, 0, 255) => PixelKind.Door,
        _ => PixelKind.Unknown,
    };

    /// <summary>
    /// Znaczenie → kolor. Potrzebne przy rysowaniu rzutu z naniesioną trasą,
    /// a także do generowania planów testowych.
    /// </summary>
    /// <remarks>
    /// <see cref="PixelKind.Unknown"/> nie ma własnego koloru w kontrakcie — zwracany jest cyjan,
    /// jedyny wolny narożnik sześcianu RGB, żeby piksele spoza kontraktu rzucały się w oczy
    /// na podglądzie dla administratora.
    /// </remarks>
    public static (byte R, byte G, byte B) ToRgb(PixelKind kind) => kind switch
    {
        PixelKind.Floor => (255, 255, 255),
        PixelKind.Wall => (0, 0, 0),
        PixelKind.Exit => (0, 255, 0),
        PixelKind.DangerZone => (255, 0, 0),
        PixelKind.Stairs => (0, 0, 255),
        PixelKind.Start => (255, 255, 0),
        PixelKind.Door => (255, 0, 255),
        _ => (0, 255, 255),
    };
}
