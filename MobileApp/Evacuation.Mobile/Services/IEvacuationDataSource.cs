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

    Task<DangerAlertResult> ReportDangerZoneAsync(
        Guid floorPlanId,
        Guid roomId,
        string kind,
        CancellationToken ct = default);
}
