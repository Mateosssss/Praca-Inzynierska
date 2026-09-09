using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Źródło danych dla ekranów aplikacji. Na tym etapie implementuje je
/// <see cref="MockEvacuationDataSource"/>; docelowo dojdzie implementacja HTTP uderzająca
/// w Evacuation.Api. Interfejs celowo mówi o pomieszczeniach i trasach, a nie o endpointach,
/// żeby podmiana nie dotknęła ViewModeli.
/// </summary>
public interface IEvacuationDataSource
{
    Task<IReadOnlyList<BuildingSummary>> GetNearbyBuildingsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<FloorSummary>> GetFloorsAsync(Guid buildingId, CancellationToken ct = default);

    Task<IReadOnlyList<RoomSummary>> GetRoomsAsync(Guid buildingId, int floorNumber, CancellationToken ct = default);

    Task<EvacuationRoute> GetRouteAsync(Guid buildingId, int floorNumber, int startRegionId, CancellationToken ct = default);

    Task<DangerAlertResult> ReportDangerZoneAsync(Guid buildingId, int floorNumber, int regionId, CancellationToken ct = default);
}
