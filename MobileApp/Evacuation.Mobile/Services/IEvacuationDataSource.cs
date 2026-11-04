using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Źródło danych dla ekranów aplikacji. Implementują je <see cref="MockEvacuationDataSource"/>
/// oraz <see cref="ApiEvacuationDataSource"/> uderzający w Evacuation.Api.
/// Interfejs mówi o pomieszczeniach i trasach, a nie o endpointach, żeby podmiana źródła
/// nie dotykała ViewModeli.
/// </summary>
public interface IEvacuationDataSource
{
    Task<IReadOnlyList<BuildingSummary>> GetNearbyBuildingsAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default);

    Task<IReadOnlyList<FloorSummary>> GetFloorsAsync(Guid buildingId, CancellationToken ct = default);

    Task<IReadOnlyList<RoomSummary>> GetRoomsAsync(Guid floorPlanId, CancellationToken ct = default);

    Task<EvacuationRoute> GetRouteAsync(Guid floorPlanId, Guid startRoomId, CancellationToken ct = default);

    /// <summary>
    /// Pomieszczenie pod wskazanym pikselem rzutu albo <c>null</c>, gdy trafiono w ścianę,
    /// drzwi lub poza plan.
    /// </summary>
    Task<Guid?> FindRoomAtAsync(Guid floorPlanId, int x, int y, CancellationToken ct = default);

    /// <summary>
    /// Adres obrazu rzutu albo <c>null</c>, gdy źródło go nie udostępnia.
    /// Dzięki temu ViewModel nie musi znać konfiguracji połączenia.
    /// </summary>
    string? GetPlanImageUrl(Guid floorPlanId);

    Task<DangerAlertResult> ReportDangerZoneAsync(
        Guid floorPlanId,
        Guid roomId,
        string kind,
        CancellationToken ct = default);
}
