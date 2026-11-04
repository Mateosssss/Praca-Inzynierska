using Evacuation.Application.Graph;
using Evacuation.Application.Routing;
using Evacuation.Application.Segmentation;

namespace Evacuation.Application.Tests;

public class EvacuationRouterTests
{
    private static FloorGraph Build(params string[] rows)
    {
        var plan = PlanBuilder.FromAscii(rows);

        return GraphBuilder.Build(plan, RoomSegmenter.Segment(plan));
    }

    /// <summary>Trzy sale w szeregu, wyjście w prawej skrajnej.</summary>
    private static FloorGraph TrzySaleWSzeregu() => Build(
        "#############",
        "#...+...+..E#",
        "#...#...#...#",
        "#############");

    [Fact]
    public void Z_sali_sasiadujacej_z_wyjsciem_trasa_ma_dwa_pomieszczenia()
    {
        var graph = Build(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        var route = EvacuationRouter.FindRoute(graph, startRoomId: 0);

        Assert.NotNull(route);
        Assert.Equal([0, 1], route.RoomIds);
        Assert.Equal(1, route.ExitRoomId);
        Assert.Equal(1, route.SegmentCount);
        Assert.Equal(2 * Math.Sqrt(5), route.LengthPixels, precision: 9);
    }

    [Fact]
    public void Stojac_juz_w_sali_z_wyjsciem_trasa_jest_zerowa()
    {
        var graph = Build(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        var route = EvacuationRouter.FindRoute(graph, startRoomId: 1);

        Assert.NotNull(route);
        Assert.True(route.AlreadyAtExit);
        Assert.Equal(0, route.LengthPixels);
    }

    [Fact]
    public void Trasa_prowadzi_przez_sale_posrednia()
    {
        var route = EvacuationRouter.FindRoute(TrzySaleWSzeregu(), startRoomId: 0);

        Assert.NotNull(route);
        Assert.Equal([0, 1, 2], route.RoomIds);
        Assert.Equal(2, route.ExitRoomId);
        Assert.Equal(8, route.LengthPixels, precision: 9);
    }

    [Fact]
    public void Jeden_przebieg_wypelnia_tablice_dla_calego_pietra()
    {
        var table = EvacuationRouter.BuildTable(TrzySaleWSzeregu());

        // Jedno liczenie, a odpowiedź gotowa dla każdej sali na piętrze.
        Assert.Equal(8, table.RouteFrom(0)!.LengthPixels, precision: 9);
        Assert.Equal(4, table.RouteFrom(1)!.LengthPixels, precision: 9);
        Assert.Equal(0, table.RouteFrom(2)!.LengthPixels, precision: 9);
    }

    [Fact]
    public void Przy_dwoch_wyjsciach_wybierane_jest_blizsze()
    {
        var graph = Build(
            "###############",
            "#E..+...+....E#",
            "#...#...#.....#",
            "###############");

        var route = EvacuationRouter.FindRoute(graph, startRoomId: 1);

        // Do lewego wyjścia jest 4, do prawego 5.
        Assert.NotNull(route);
        Assert.Equal(0, route.ExitRoomId);
        Assert.Equal(4, route.LengthPixels, precision: 9);
    }

    [Fact]
    public void Strefa_zagrozenia_kieruje_trase_do_dalszego_wyjscia()
    {
        // Ten sam układ, ale lewa sala ma narysowaną strefę niebezpieczną.
        var graph = Build(
            "###############",
            "#E.D+...+....E#",
            "#...#...#.....#",
            "###############");

        var route = EvacuationRouter.FindRoute(graph, startRoomId: 1);

        Assert.NotNull(route);
        Assert.Equal(2, route.ExitRoomId);
        Assert.Equal(5, route.LengthPixels, precision: 9);
    }

    [Fact]
    public void Gdy_jedyna_droga_wiedzie_przez_zagrozenie_trasa_mimo_to_powstaje()
    {
        // Jedyne wyjście leży za salą ze strefą zagrożenia. System ma poprowadzić,
        // a nie odpowiedzieć „brak trasy”.
        var graph = Build(
            "#############",
            "#...+.D.+..E#",
            "#...#...#...#",
            "#############");

        var route = EvacuationRouter.FindRoute(graph, startRoomId: 0);

        Assert.NotNull(route);
        Assert.Equal([0, 1, 2], route.RoomIds);
        Assert.True(route.PassesThroughDanger);

        // Rzeczywista długość pozostaje bez zmian — kara dotyczy tylko kosztu wyboru.
        Assert.Equal(8, route.LengthPixels, precision: 9);
        Assert.True(route.CostPixels > route.LengthPixels);
    }

    [Fact]
    public void Sala_odcieta_od_wyjsc_nie_ma_trasy_i_jest_raportowana()
    {
        var graph = Build(
            "#########",
            "#..#....#",
            "#..#...E#",
            "#########");

        var table = EvacuationRouter.BuildTable(graph);

        Assert.Null(table.RouteFrom(0));
        Assert.False(table.CanReachExit(0));
        Assert.Equal([0], table.RoomsWithoutExit());

        Assert.NotNull(table.RouteFrom(1));
    }
}
