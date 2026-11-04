namespace Evacuation.Domain;

/// <summary>
/// Pomieszczenie wykryte przez segmentację — wierzchołek grafu kondygnacji.
/// </summary>
public class Room
{
    public Guid Id { get; set; }

    public Guid FloorPlanId { get; set; }

    public FloorPlan? FloorPlan { get; set; }

    /// <summary>
    /// Numer regionu nadany przez segmentację.
    /// </summary>
    /// <remarks>
    /// Celowo obok <see cref="Id"/>, a nie zamiast niego. W bazie relacje opierają się na
    /// identyfikatorze, ale algorytmy posługują się indeksami od zera i na nich stoi niezmiennik
    /// „region o numerze i leży na i-tej pozycji listy”. Bez zapisanego numeru nie odtworzyłoby się
    /// grafu z bazy w tej samej postaci, w jakiej został policzony.
    /// </remarks>
    public int RegionId { get; set; }

    /// <summary>
    /// Nazwa nadana przez administratora, np. „Sala 118 — wykładowa”.
    /// <c>null</c> oznacza pomieszczenie jeszcze nieopisane — aplikacja pokaże wtedy numer regionu.
    /// </summary>
    public string? Label { get; set; }

    public int CentroidX { get; set; }

    public int CentroidY { get; set; }

    /// <summary>Powierzchnia w pikselach. Po przeliczeniu przez skalę rzutu daje metry kwadratowe.</summary>
    public int PixelCount { get; set; }

    /// <summary>Czy w pomieszczeniu jest wyjście ewakuacyjne.</summary>
    public bool IsExit { get; set; }

    /// <summary>
    /// Czy pomieszczenie jest klatką schodową lub windą. Tym łączy się kondygnacje —
    /// pokoje z tą flagą na sąsiednich piętrach dostają krawędź międzypiętrową.
    /// </summary>
    public bool IsStaircase { get; set; }

    /// <summary>Czy na rzucie zaznaczono tu punkt startowy.</summary>
    public bool IsStart { get; set; }

    /// <summary>
    /// Czy strefa niebezpieczna jest narysowana na samym rzucie — zagrożenie trwałe,
    /// w odróżnieniu od zgłoszeń w <see cref="DangerZone"/>, które są chwilowe.
    /// </summary>
    public bool HasPaintedDanger { get; set; }

    public ICollection<DangerZone> DangerZones { get; set; } = new List<DangerZone>();
}
