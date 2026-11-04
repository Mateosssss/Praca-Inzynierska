using Evacuation.Application.Graph;
using Evacuation.Application.Imaging;
using Evacuation.Application.Routing;
using Evacuation.Application.Segmentation;
using Evacuation.Infrastructure.Imaging;

namespace Evacuation.Infrastructure.Tests;

public class PngPlanImageDecoderTests
{
    private readonly PngPlanImageDecoder _decoder = new();

    [Fact]
    public void Dekoder_odczytuje_wymiary_rzutu()
    {
        using var png = PlanImage.Png(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        var plan = _decoder.Decode(png);

        Assert.Equal(9, plan.Width);
        Assert.Equal(5, plan.Height);
    }

    [Theory]
    [InlineData(0, 0, PixelKind.Wall)]
    [InlineData(1, 1, PixelKind.Floor)]
    [InlineData(4, 1, PixelKind.Door)]
    [InlineData(7, 3, PixelKind.Exit)]
    public void Dekoder_odczytuje_kontrakt_kolorow(int x, int y, PixelKind oczekiwany)
    {
        using var png = PlanImage.Png(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        var plan = _decoder.Decode(png);

        Assert.Equal(oczekiwany, plan[x, y]);
    }

    [Fact]
    public void Wszystkie_kolory_kontraktu_przechodza_tam_i_z_powrotem()
    {
        using var png = PlanImage.Png("#.EDSY+?");

        var plan = _decoder.Decode(png);

        Assert.Equal(PixelKind.Wall, plan[0, 0]);
        Assert.Equal(PixelKind.Floor, plan[1, 0]);
        Assert.Equal(PixelKind.Exit, plan[2, 0]);
        Assert.Equal(PixelKind.DangerZone, plan[3, 0]);
        Assert.Equal(PixelKind.Stairs, plan[4, 0]);
        Assert.Equal(PixelKind.Start, plan[5, 0]);
        Assert.Equal(PixelKind.Door, plan[6, 0]);
        Assert.Equal(PixelKind.Unknown, plan[7, 0]);
    }

    [Fact]
    public void Jpeg_jest_odrzucany_mimo_ze_jest_poprawnym_obrazem()
    {
        using var jpeg = PlanImage.Jpeg(
            "#########",
            "#...+...#",
            "#########");

        var blad = Assert.Throws<InvalidPlanImageException>(() => _decoder.Decode(jpeg));

        Assert.Contains("PNG", blad.Message);
    }

    [Fact]
    public void Plik_ktory_nie_jest_obrazem_jest_odrzucany()
    {
        using var smieci = new MemoryStream("to nie jest obrazek"u8.ToArray());

        Assert.Throws<InvalidPlanImageException>(() => _decoder.Decode(smieci));
    }

    [Fact]
    public void Prawdziwy_png_przechodzi_caly_pipeline_az_do_trasy()
    {
        using var png = PlanImage.Png(
            "#############",
            "#...+...+..E#",
            "#...#...#...#",
            "#############");

        var plan = _decoder.Decode(png);
        var rooms = RoomSegmenter.Segment(plan);
        var graph = GraphBuilder.Build(plan, rooms);
        var route = EvacuationRouter.FindRoute(graph, startRoomId: 0);

        Assert.Equal(3, rooms.Regions.Count);
        Assert.Equal(2, graph.Edges.Count);
        Assert.Empty(graph.Warnings);

        Assert.NotNull(route);
        Assert.Equal([0, 1, 2], route.RoomIds);
        Assert.Equal(8, route.LengthPixels, precision: 9);
    }
}
