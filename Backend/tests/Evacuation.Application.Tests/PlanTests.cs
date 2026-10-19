using Evacuation.Application.Imaging;

namespace Evacuation.Application.Tests;

public class PlanTests
{
    /// <summary>
    /// Plansza kontrolna: dwie sale rozdzielone pionową ścianą, jedne drzwi w tej ścianie,
    /// wyjście w prawej sali. Celowo nie jest kwadratowa — gdyby gdzieś zamienić x z y,
    /// odczyt wyjdzie poza tablicę i test padnie głośno, zamiast po cichu zwrócić zły piksel.
    /// </summary>
    private static Plan DwieSaleZDrzwiami() => PlanBuilder.FromAscii(
        "#########",
        "#...+...#",
        "#...#...#",
        "#...#..E#",
        "#########");

    [Fact]
    public void Plan_zbudowany_z_ascii_ma_poprawne_wymiary()
    {
        var plan = DwieSaleZDrzwiami();

        Assert.Equal(9, plan.Width);
        Assert.Equal(5, plan.Height);
    }

    [Theory]
    [InlineData(0, 0, PixelKind.Wall)]
    [InlineData(1, 1, PixelKind.Floor)]
    [InlineData(4, 1, PixelKind.Door)]
    [InlineData(7, 3, PixelKind.Exit)]
    public void Plan_zbudowany_z_ascii_ma_poprawne_piksele(int x, int y, PixelKind oczekiwany)
    {
        var plan = DwieSaleZDrzwiami();

        Assert.Equal(oczekiwany, plan[x, y]);
    }

    [Fact]
    public void Wiersze_roznej_dlugosci_sa_odrzucane()
    {
        var blad = Assert.Throws<ArgumentException>(() => PlanBuilder.FromAscii(
            "####",
            "#..#",
            "###"));

        Assert.Contains("Wiersz 2", blad.Message);
    }

    [Fact]
    public void Nieznany_znak_jest_odrzucany()
    {
        var blad = Assert.Throws<ArgumentException>(() => PlanBuilder.FromAscii(
            "###",
            "#X#",
            "###"));

        Assert.Contains("'X'", blad.Message);
    }
}
