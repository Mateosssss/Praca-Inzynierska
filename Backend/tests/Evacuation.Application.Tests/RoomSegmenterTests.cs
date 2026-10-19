using Evacuation.Application.Imaging;
using Evacuation.Application.Segmentation;

namespace Evacuation.Application.Tests;

public class RoomSegmenterTests
{
    /// <summary>Dwie sale rozdzielone ścianą, jedne drzwi, wyjście w prawej sali.</summary>
    private static Plan DwieSaleZDrzwiami() => PlanBuilder.FromAscii(
        "#########",
        "#...+...#",
        "#...#...#",
        "#...#..E#",
        "#########");

    [Fact]
    public void Drzwi_nie_sklejaja_sal_w_jeden_region()
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        Assert.Equal(2, wynik.Regions.Count);
    }

    [Fact]
    public void Oba_regiony_maja_po_dziewiec_pikseli()
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        Assert.Equal(9, wynik.Regions[0].PixelCount);
        Assert.Equal(9, wynik.Regions[1].PixelCount);
    }

    [Fact]
    public void Centroidy_leza_w_srodkach_sal()
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        Assert.Equal((2, 2), (wynik.Regions[0].CentroidX, wynik.Regions[0].CentroidY));
        Assert.Equal((6, 2), (wynik.Regions[1].CentroidX, wynik.Regions[1].CentroidY));
    }

    [Fact]
    public void Flage_wyjscia_dostaje_tylko_sala_z_wyjsciem()
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        Assert.False(wynik.Regions[0].HasExit);
        Assert.True(wynik.Regions[1].HasExit);
    }

    [Theory]
    [InlineData(1, 1, 0)]   // lewa sala
    [InlineData(5, 1, 1)]   // prawa sala
    [InlineData(7, 3, 1)]   // wyjście należy do prawej sali
    [InlineData(4, 1, -1)]  // drzwi — do żadnego regionu
    [InlineData(0, 0, -1)]  // ściana
    public void Mapa_etykiet_wskazuje_wlasciwe_regiony(int x, int y, int oczekiwany)
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        Assert.Equal(oczekiwany, wynik.RegionAt(x, y));
    }

    [Fact]
    public void Numer_regionu_odpowiada_pozycji_na_liscie()
    {
        var wynik = RoomSegmenter.Segment(DwieSaleZDrzwiami());

        for (var i = 0; i < wynik.Regions.Count; i++)
        {
            Assert.Equal(i, wynik.Regions[i].Id);
        }
    }
}
