namespace Evacuation.Application.Segmentation;

/// <summary>
/// Jedno pomieszczenie znalezione przez segmentację — przyszły wierzchołek grafu,
/// a po zapisaniu do bazy encja <c>Room</c>.
/// </summary>
/// <param name="Id">Numer nadany przez segmentację. Odpowiada pozycji na liście regionów.</param>
/// <param name="PixelCount">Liczba pikseli regionu. Posłuży do wyliczenia powierzchni i do odrzucania śmieci.</param>
/// <param name="CentroidX">Środek regionu w poziomie — z niego liczone są wagi krawędzi.</param>
/// <param name="CentroidY">Środek regionu w pionie.</param>
/// <param name="HasExit">Czy w regionie jest piksel wyjścia ewakuacyjnego.</param>
/// <param name="HasStairs">Czy w regionie są schody — tym łączy się piętra.</param>
/// <param name="HasDanger">Czy w regionie jest strefa niebezpieczna.</param>
/// <param name="HasStart">Czy w regionie jest zaznaczony punkt startowy.</param>
public sealed record Region(
    int Id,
    int PixelCount,
    int CentroidX,
    int CentroidY,
    bool HasExit,
    bool HasStairs,
    bool HasDanger,
    bool HasStart);
