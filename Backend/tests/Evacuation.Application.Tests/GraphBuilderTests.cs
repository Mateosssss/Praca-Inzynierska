using Evacuation.Application.Graph;
using Evacuation.Application.Imaging;
using Evacuation.Application.Segmentation;

namespace Evacuation.Application.Tests;

public class GraphBuilderTests
{
    private static FloorGraph Build(params string[] rows)
    {
        var plan = PlanBuilder.FromAscii(rows);
        var rooms = RoomSegmenter.Segment(plan);

        return GraphBuilder.Build(plan, rooms);
    }

    [Fact]
    public void Jedne_drzwi_daja_jedna_krawedz_miedzy_salami()
    {
        var graph = Build(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        var edge = Assert.Single(graph.Edges);

        Assert.Equal(0, edge.FromRoomId);
        Assert.Equal(1, edge.ToRoomId);
        Assert.Empty(graph.Warnings);
    }

    [Fact]
    public void Waga_liczona_jest_przez_drzwi_a_nie_w_linii_prostej()
    {
        var graph = Build(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...#..E#",
            "#########");

        // Centroidy sal to (2,2) i (6,2), środek drzwi (4,1).
        // Trasa (2,2) → (4,1) → (6,2) to dwa odcinki o długości √5 każdy.
        // W linii prostej między centroidami wyszłoby 4 — i przechodziłoby przez ścianę.
        Assert.Equal(2 * Math.Sqrt(5), Assert.Single(graph.Edges).WeightPixels, precision: 9);
    }

    [Fact]
    public void Wielopikselowe_drzwi_i_gruba_sciana_daja_nadal_jedna_krawedz()
    {
        var graph = Build(
            "#############",
            "#....##.....#",
            "#....##.....#",
            "#....++.....#",
            "#....++.....#",
            "#....++.....#",
            "#....##.....#",
            "#....##....E#",
            "#############");

        Assert.Equal(2, graph.Rooms.Count);

        var edge = Assert.Single(graph.Edges);

        Assert.Equal(0, edge.FromRoomId);
        Assert.Equal(1, edge.ToRoomId);
        Assert.Empty(graph.Warnings);
    }

    [Fact]
    public void Dwoje_drzwi_miedzy_tymi_samymi_salami_daje_jedna_krawedz()
    {
        var graph = Build(
            "#########",
            "#...+...#",
            "#...#...#",
            "#...+...#",
            "#########");

        Assert.Equal(2, graph.Rooms.Count);
        Assert.Single(graph.Edges);
    }

    [Fact]
    public void Drzwi_na_zewnatrz_nie_tworza_krawedzi_ani_ostrzezenia()
    {
        var graph = Build(
            "#####",
            "#...#",
            "#...+",
            "#####");

        Assert.Single(graph.Rooms);
        Assert.Empty(graph.Edges);
        Assert.Empty(graph.Warnings);
    }

    [Fact]
    public void Drzwi_stykajace_sie_z_trzema_salami_zglaszane_sa_jako_usterka()
    {
        // Trzy pomieszczenia stykające się z tymi samymi drzwiami — w takim miejscu
        // nie da się jednoznacznie powiedzieć, które dwa łączy przejście.
        var graph = Build(
            "#####",
            "##.##",
            "#.+.#",
            "#####");

        Assert.Equal(3, graph.Rooms.Count);
        Assert.Empty(graph.Edges);

        var warning = Assert.Single(graph.Warnings);

        Assert.Contains("3 pomieszczeniami", warning.Message);
    }

    [Fact]
    public void Plan_bez_drzwi_daje_graf_bez_krawedzi()
    {
        var graph = Build(
            "#######",
            "#..#..#",
            "#..#..#",
            "#######");

        Assert.Equal(2, graph.Rooms.Count);
        Assert.Empty(graph.Edges);
    }
}
