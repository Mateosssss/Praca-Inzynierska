using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Dane odwzorowujące makiety z katalogu <c>Makieta/</c>. Pozwalają przeklikać cały przepływ
/// bez uruchamiania backendu — przydatne przy pracy nad samym interfejsem.
/// Identyfikatory są stałe, więc kolejne uruchomienia dają ten sam wynik.
/// </summary>
public sealed class MockEvacuationDataSource : IEvacuationDataSource
{
    private static readonly Guid WydzialFizyki = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Pietro2 = new("22222222-0000-0000-0000-000000000002");

    private static readonly BuildingSummary[] Buildings =
    [
        new(WydzialFizyki, "Wydział Fizyki, Astronomii i Informatyki", "ul. Grudziądzka 5, Toruń", 84, 6, "Aktywny alarm · piętro 2"),
        new(new Guid("22222222-2222-2222-2222-222222222222"), "Collegium Humanisticum", "ul. Bojarskiego 1, Toruń", 310, 4, null),
        new(new Guid("33333333-3333-3333-3333-333333333333"), "Biblioteka Uniwersytecka", "ul. Gagarina 13, Toruń", 640, 3, null),
    ];

    private static readonly FloorSummary[] Floors =
    [
        new(new Guid("22222222-0000-0000-0000-000000000003"), 3, "Piętro 3", 14, 2, 0, 0, true),
        new(Pietro2, 2, "Piętro 2", 18, 2, 0, 1, true),
        new(new Guid("22222222-0000-0000-0000-000000000001"), 1, "Piętro 1", 16, 2, 0, 0, true),
        new(new Guid("22222222-0000-0000-0000-000000000000"), 0, "Parter", 21, 4, 4, 0, true),
        new(new Guid("22222222-0000-0000-0000-00000000000f"), -1, "Piwnica", 0, 0, 0, 0, false),
    ];

    private static readonly RoomSummary[] Rooms =
    [
        new(RoomId(12), 12, "Sala 110 — laboratorium", "Skrzydło wschodnie", "region #12 · 46 m²", false, false, false),
        new(RoomId(13), 13, "Sala 112 — seminaryjna", "Skrzydło wschodnie", "region #13 · 28 m²", false, false, false),
        new(RoomId(17), 17, "Sala 118 — wykładowa", "Skrzydło wschodnie", "region #17 · centroid 246, 118", false, false, false),
        new(RoomId(18), 18, "Sala 119 — pracownia", "Skrzydło wschodnie", "region #18 · 34 m²", false, false, false),
        new(RoomId(2), 2, "Korytarz A", "Ciągi komunikacyjne", "region #2 · sąsiaduje z 9 pomieszczeniami", false, false, false),
        new(RoomId(4), 4, "Klatka B", "Ciągi komunikacyjne", "region #4 · łączy piętra 0–3", false, true, false),
    ];

    private static Guid RoomId(int regionId) => new($"33333333-0000-0000-0000-{regionId:D12}");

    public async Task<IReadOnlyList<BuildingSummary>> GetNearbyBuildingsAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        await Task.Delay(400, ct);

        return Buildings;
    }

    public async Task<IReadOnlyList<FloorSummary>> GetFloorsAsync(Guid buildingId, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);

        return Floors;
    }

    public async Task<IReadOnlyList<RoomSummary>> GetRoomsAsync(Guid floorPlanId, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);

        return Rooms;
    }

    public async Task<EvacuationRoute> GetRouteAsync(
        Guid floorPlanId,
        Guid startRoomId,
        CancellationToken ct = default)
    {
        await Task.Delay(350, ct);

        return new EvacuationRoute(
            Headline: "Najbliższe wyjście",
            TotalMeters: 62,
            Summary: "wyjście wschodnie · 4 odcinki · ok. 50 s marszu",
            Steps:
            [
                new RouteStep(1, "Wyjdź z sali 118 na korytarz A"),
                new RouteStep(2, "Skręć w prawo, idź 38 m wzdłuż korytarza"),
                new RouteStep(3, "Wejdź do sali 102, drzwi po prawej"),
                new RouteStep(4, "Wyjście ewakuacyjne na zewnątrz"),
            ]);
    }

    public async Task<DangerAlertResult> ReportDangerZoneAsync(
        Guid floorPlanId,
        Guid roomId,
        string kind,
        CancellationToken ct = default)
    {
        await Task.Delay(500, ct);

        var alert = new DangerZoneAlert(
            Title: "Alarm · piętro 2",
            Description: "Strefa zagrożenia na korytarzu A",
            Kind: kind,
            ReportedAt: new TimeOnly(12, 4, 31),
            CostMultiplier: "×25 na krawędziach strefy",
            RecalculationStats: "14 ms · 41 wierzchołków");

        var route = new EvacuationRoute(
            Headline: "Nowa trasa: klatka A",
            TotalMeters: 88,
            Summary: "klatka A · zejście na parter · wyjście główne",
            Steps:
            [
                new RouteStep(1, "Wyjdź z sali 118 i skręć w lewo, z dala od korytarza A"),
                new RouteStep(2, "Idź 24 m do klatki A"),
                new RouteStep(3, "Zejdź na parter"),
                new RouteStep(4, "Wyjście główne na zewnątrz"),
            ],
            ComparisonNote: "+26 m względem poprzedniej · zejście na parter, wyjście główne");

        return new DangerAlertResult(alert, route);
    }
}
