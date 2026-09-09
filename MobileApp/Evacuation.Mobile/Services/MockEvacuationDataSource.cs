using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Dane odwzorowujące makiety z katalogu <c>Makieta/</c>. Pozwalają przeklikać cały przepływ
/// zanim powstanie Evacuation.Api. Każda metoda ma symbolowe opóźnienie, żeby wskaźniki
/// ładowania na ekranach były widoczne w działaniu.
/// </summary>
public sealed class MockEvacuationDataSource : IEvacuationDataSource
{
    private static readonly Guid WydzialFizyki = new("11111111-1111-1111-1111-111111111111");

    private static readonly BuildingSummary[] Buildings =
    [
        new(WydzialFizyki,
            "Wydział Fizyki, Astronomii i Informatyki",
            "ul. Grudziądzka 5, Toruń",
            DistanceMeters: 84,
            FloorCount: 6,
            AlarmNote: "Aktywny alarm · piętro 2"),
        new(new Guid("22222222-2222-2222-2222-222222222222"),
            "Collegium Humanisticum",
            "ul. Bojarskiego 1, Toruń",
            DistanceMeters: 310,
            FloorCount: 4,
            AlarmNote: null),
        new(new Guid("33333333-3333-3333-3333-333333333333"),
            "Biblioteka Uniwersytecka",
            "ul. Gagarina 13, Toruń",
            DistanceMeters: 640,
            FloorCount: 3,
            AlarmNote: null),
    ];

    private static readonly FloorSummary[] Floors =
    [
        new(3, "Piętro 3", RoomCount: 14, StaircaseCount: 2, ExitCount: 0, DangerZoneCount: 0, HasPlan: true),
        new(2, "Piętro 2", RoomCount: 18, StaircaseCount: 2, ExitCount: 0, DangerZoneCount: 1, HasPlan: true),
        new(1, "Piętro 1", RoomCount: 16, StaircaseCount: 2, ExitCount: 0, DangerZoneCount: 0, HasPlan: true),
        new(0, "Parter", RoomCount: 21, StaircaseCount: 4, ExitCount: 4, DangerZoneCount: 0, HasPlan: true),
        new(-1, "Piwnica", RoomCount: 0, StaircaseCount: 0, ExitCount: 0, DangerZoneCount: 0, HasPlan: false),
    ];

    private static readonly RoomSummary[] Rooms =
    [
        new(12, "Sala 110 — laboratorium", "Skrzydło wschodnie", "region #12 · 46 m²"),
        new(13, "Sala 112 — seminaryjna", "Skrzydło wschodnie", "region #13 · 28 m²"),
        new(17, "Sala 118 — wykładowa", "Skrzydło wschodnie", "region #17 · centroid 246, 118"),
        new(18, "Sala 119 — pracownia", "Skrzydło wschodnie", "region #18 · 34 m²"),
        new(2, "Korytarz A", "Ciągi komunikacyjne", "region #2 · sąsiaduje z 9 pomieszczeniami"),
        new(4, "Klatka B", "Ciągi komunikacyjne", "region #4 · łączy piętra 0–3"),
    ];

    public async Task<IReadOnlyList<BuildingSummary>> GetNearbyBuildingsAsync(CancellationToken ct = default)
    {
        await Task.Delay(400, ct);
        return Buildings;
    }

    public async Task<IReadOnlyList<FloorSummary>> GetFloorsAsync(Guid buildingId, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);
        return Floors;
    }

    public async Task<IReadOnlyList<RoomSummary>> GetRoomsAsync(Guid buildingId, int floorNumber, CancellationToken ct = default)
    {
        await Task.Delay(250, ct);
        return Rooms;
    }

    public async Task<EvacuationRoute> GetRouteAsync(Guid buildingId, int floorNumber, int startRegionId, CancellationToken ct = default)
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

    public async Task<DangerAlertResult> ReportDangerZoneAsync(Guid buildingId, int floorNumber, int regionId, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);

        var alert = new DangerZoneAlert(
            Title: "Alarm · piętro 2",
            Description: "Strefa zagrożenia na korytarzu A",
            Kind: "pożar",
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
