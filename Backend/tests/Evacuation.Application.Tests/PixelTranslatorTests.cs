using Evacuation.Application.Imaging;

namespace Evacuation.Application.Tests;

public class PixelTranslatorTests
{
    [Theory]
    [InlineData(255, 255, 255, PixelKind.Floor)]
    [InlineData(0, 0, 0, PixelKind.Wall)]
    [InlineData(0, 255, 0, PixelKind.Exit)]
    [InlineData(255, 0, 0, PixelKind.DangerZone)]
    [InlineData(0, 0, 255, PixelKind.Stairs)]
    [InlineData(255, 255, 0, PixelKind.Start)]
    [InlineData(255, 0, 255, PixelKind.Door)]
    public void Kolory_kontraktu_maja_swoje_znaczenia(byte r, byte g, byte b, PixelKind oczekiwany) =>
        Assert.Equal(oczekiwany, PixelTranslator.FromRgb(r, g, b));

    [Theory]
    [InlineData(254, 254, 254)]  // biel po kompresji stratnej
    [InlineData(254, 0, 0)]      // czerwień po kompresji stratnej
    [InlineData(128, 128, 128)]  // szary — np. cień z antyaliasingu
    [InlineData(0, 255, 255)]    // cyjan — wolny narożnik, poza kontraktem
    public void Kolor_spoza_kontraktu_jest_nieznany(byte r, byte g, byte b) =>
        Assert.Equal(PixelKind.Unknown, PixelTranslator.FromRgb(r, g, b));

    [Theory]
    [InlineData(PixelKind.Floor)]
    [InlineData(PixelKind.Wall)]
    [InlineData(PixelKind.Exit)]
    [InlineData(PixelKind.DangerZone)]
    [InlineData(PixelKind.Stairs)]
    [InlineData(PixelKind.Start)]
    [InlineData(PixelKind.Door)]
    public void Zamiana_na_kolor_i_z_powrotem_zwraca_to_samo_znaczenie(PixelKind kind)
    {
        var (r, g, b) = PixelTranslator.ToRgb(kind);

        Assert.Equal(kind, PixelTranslator.FromRgb(r, g, b));
    }

    [Fact]
    public void Kazde_znaczenie_ma_wlasny_kolor()
    {
        PixelKind[] kontrakt =
        [
            PixelKind.Floor, PixelKind.Wall, PixelKind.Exit, PixelKind.DangerZone,
            PixelKind.Stairs, PixelKind.Start, PixelKind.Door,
        ];

        var kolory = kontrakt.Select(PixelTranslator.ToRgb).ToHashSet();

        Assert.Equal(kontrakt.Length, kolory.Count);
    }
}
