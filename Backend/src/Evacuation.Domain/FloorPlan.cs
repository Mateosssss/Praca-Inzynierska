namespace Evacuation.Domain;

/// <summary>
/// Rzut jednej kondygnacji wraz z grafem wyliczonym z obrazu.
/// </summary>
public class FloorPlan
{
    public Guid Id { get; set; }

    public Guid BuildingId { get; set; }

    public Building? Building { get; set; }

    /// <summary>
    /// Numer kondygnacji ze znakiem — parter to 0, piwnice mają wartości ujemne.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Nazwa pokazywana użytkownikowi. Osobne pole, bo nie zawsze wynika z numeru:
    /// antresole oraz numeracja liczona od parteru albo od pierwszego piętra różnią się
    /// między budynkami.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Oryginalny plik PNG wgrany przez administratora.</summary>
    public byte[] ImagePng { get; set; } = [];

    public int WidthPx { get; set; }

    public int HeightPx { get; set; }

    /// <summary>
    /// Ile pikseli rzutu przypada na metr. Bez tego wagi krawędzi pozostają w pikselach
    /// i nie da się pokazać użytkownikowi dystansu w metrach. Wartość należy do rzutu,
    /// a nie do budynku — poszczególne piętra bywają skanowane w różnych rozdzielczościach.
    /// </summary>
    public double PixelsPerMeter { get; set; }

    public FloorPlanStatus Status { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public ICollection<RoomEdge> Edges { get; set; } = new List<RoomEdge>();
}

/// <summary>Etap przetwarzania rzutu.</summary>
public enum FloorPlanStatus
{
    /// <summary>Obraz wgrany, graf jeszcze nie policzony.</summary>
    Uploaded = 0,

    /// <summary>Graf gotowy — dopiero teraz piętro nadaje się do wyznaczania tras.</summary>
    Processed = 1,

    /// <summary>Przetwarzanie nie powiodło się, np. rzut łamie kontrakt kolorów.</summary>
    Failed = 2,
}
