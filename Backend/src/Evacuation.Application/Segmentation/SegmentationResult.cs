namespace Evacuation.Application.Segmentation;

/// <summary>
/// Wynik segmentacji rzutu: lista pomieszczeń oraz mapa mówiąca, który piksel
/// należy do którego z nich.
/// </summary>
/// <remarks>
/// Mapa etykiet jest potrzebna do wykrywania drzwi: biorąc piksel drzwi i pytając
/// o regiony jego sąsiadów, dowiadujemy się, które pomieszczenia łączy.
/// </remarks>
public sealed class SegmentationResult
{
    private readonly int[] _labels;

    public SegmentationResult(int width, int height, IReadOnlyList<Region> regions, int[] labels)
    {
        if (labels.Length != width * height)
        {
            throw new ArgumentException(
                $"Mapa etykiet ma {labels.Length} pól, a plan {width}×{height} wymaga {width * height}.",
                nameof(labels));
        }

        Width = width;
        Height = height;
        Regions = regions;
        _labels = labels;
    }

    public int Width { get; }

    public int Height { get; }

    /// <summary>Znalezione pomieszczenia. Zachodzi <c>Regions[i].Id == i</c>.</summary>
    public IReadOnlyList<Region> Regions { get; }

    /// <summary>
    /// Numer regionu na danym pikselu albo <c>-1</c>, gdy piksel do żadnego nie należy —
    /// czyli jest ścianą, drzwiami lub kolorem spoza kontraktu.
    /// </summary>
    /// <remarks>
    /// Współrzędne spoza planu także dają <c>-1</c>. Dzięki temu przeglądanie sąsiadów
    /// piksela leżącego przy krawędzi nie wymaga osobnego sprawdzania zakresu.
    /// </remarks>
    public int RegionAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return -1;
        }

        return _labels[y * Width + x];
    }
}
