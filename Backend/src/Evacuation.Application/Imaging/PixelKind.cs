namespace Evacuation.Application.Imaging;

/// <summary>
/// Znaczenie pojedynczego piksela rzutu — nie kolor, tylko to, co kolor oznacza.
/// Algorytmy operują wyłącznie na tych wartościach, dzięki czemu nie zależą
/// od formatu obrazu ani od biblioteki dekodującej.
/// </summary>
public enum PixelKind
{
    /// <summary>
    /// Kolor spoza kontraktu. Stoi pierwsze celowo: ma wartość 0, więc świeżo utworzona
    /// tablica pikseli jest „nieznana wszędzie”, a nie „przechodnia wszędzie”.
    /// </summary>
    Unknown,

    /// <summary>Biały <c>#FFFFFF</c> — podłoga, przejście.</summary>
    Floor,

    /// <summary>Czarny <c>#000000</c> — ściana.</summary>
    Wall,

    /// <summary>Zielony <c>#00FF00</c> — wyjście ewakuacyjne.</summary>
    Exit,

    /// <summary>Czerwony <c>#FF0000</c> — strefa niebezpieczna.</summary>
    DangerZone,

    /// <summary>Niebieski <c>#0000FF</c> — schody lub winda.</summary>
    Stairs,

    /// <summary>Żółty <c>#FFFF00</c> — punkt startowy.</summary>
    Start,

    /// <summary>Magenta <c>#FF00FF</c> — drzwi między pomieszczeniami.</summary>
    Door,
}
