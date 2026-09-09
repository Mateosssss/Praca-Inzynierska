namespace Evacuation.Mobile.Models;

/// <summary>
/// Kondygnacja na ekranie „Wybierz piętro”. <paramref name="HasPlan"/> na false oznacza,
/// że administrator nie wgrał jeszcze rzutu — pozycja jest wyszarzona i nieklikalna.
/// </summary>
public sealed record FloorSummary(
    int Number,
    string Label,
    int RoomCount,
    int StaircaseCount,
    int ExitCount,
    int DangerZoneCount,
    bool HasPlan)
{
    /// <summary>„18 pomieszczeń · 2 klatki” albo „21 pomieszczeń · 4 wyjścia”.</summary>
    public string Detail
    {
        get
        {
            if (!HasPlan)
            {
                return "brak rzutu w bazie";
            }

            var parts = new List<string> { $"{RoomCount} {Odmiana(RoomCount, "pomieszczenie", "pomieszczenia", "pomieszczeń")}" };

            if (ExitCount > 0)
            {
                parts.Add($"{ExitCount} {Odmiana(ExitCount, "wyjście", "wyjścia", "wyjść")}");
            }
            else if (StaircaseCount > 0)
            {
                parts.Add($"{StaircaseCount} {Odmiana(StaircaseCount, "klatka", "klatki", "klatek")}");
            }

            return string.Join(" · ", parts);
        }
    }

    public bool HasDangerZone => DangerZoneCount > 0;

    public string DangerLabel =>
        $"{DangerZoneCount} {Odmiana(DangerZoneCount, "strefa zagrożenia", "strefy zagrożenia", "stref zagrożenia")}";

    /// <summary>
    /// Polska liczba mnoga: 1 → pojedyncza, 2–4 → mnoga, 5+ i 12–14 → dopełniacz.
    /// </summary>
    private static string Odmiana(int n, string pojedyncza, string mnoga, string dopelniacz)
    {
        if (n == 1)
        {
            return pojedyncza;
        }

        var dziesiatki = n % 100;
        var jednosci = n % 10;

        if (jednosci is >= 2 and <= 4 && dziesiatki is < 12 or > 14)
        {
            return mnoga;
        }

        return dopelniacz;
    }
}
