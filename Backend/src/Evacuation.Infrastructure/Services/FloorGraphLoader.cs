using Evacuation.Application.Graph;
using Evacuation.Application.Routing;
using Evacuation.Application.Segmentation;
using Evacuation.Domain;
using Evacuation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Infrastructure.Services;

/// <summary>
/// Odtwarza graf kondygnacji z bazy w postaci, jakiej oczekują algorytmy.
/// </summary>
public sealed class FloorGraphLoader(EvacuationDbContext db)
{
    public async Task<LoadedFloor?> LoadAsync(Guid floorPlanId, CancellationToken ct = default)
    {
        var plan = await db.FloorPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == floorPlanId, ct);

        if (plan is null)
        {
            return null;
        }

        // Sortowanie po RegionId jest tu warunkiem poprawności, a nie kosmetyką:
        // algorytmy zakładają, że region o numerze i leży na i-tej pozycji listy.
        var rooms = await db.Rooms
            .AsNoTracking()
            .Where(room => room.FloorPlanId == floorPlanId)
            .OrderBy(room => room.RegionId)
            .ToListAsync(ct);

        var edges = await db.RoomEdges
            .AsNoTracking()
            .Where(edge => edge.FloorPlanId == floorPlanId)
            .ToListAsync(ct);

        var regionByRoomId = rooms.ToDictionary(room => room.Id, room => room.RegionId);

        var regions = rooms
            .Select(room => new Region(
                room.RegionId,
                room.PixelCount,
                room.CentroidX,
                room.CentroidY,
                room.IsExit,
                room.IsStaircase,
                room.HasPaintedDanger,
                room.IsStart))
            .ToList();

        var graphEdges = edges
            .Select(edge => new Application.Graph.RoomEdge(
                regionByRoomId[edge.FromRoomId],
                regionByRoomId[edge.ToRoomId],
                edge.WeightPixels))
            .ToList();

        var graph = new FloorGraph(regions, graphEdges, []);

        return new LoadedFloor(plan, rooms, graph);
    }

    /// <summary>
    /// Buduje opcje trasowania z zagrożeń aktywnych w tej chwili.
    /// Strefy narysowane na rzucie dokłada już sam router, na podstawie flag regionów.
    /// </summary>
    public async Task<RoutingOptions> LoadRoutingOptionsAsync(Guid floorPlanId, CancellationToken ct = default)
    {
        var reported = await db.DangerZones
            .AsNoTracking()
            .Where(zone => zone.Status == DangerZoneStatus.Active
                && zone.Room != null
                && zone.Room.FloorPlanId == floorPlanId)
            .Select(zone => zone.Room!.RegionId)
            .Distinct()
            .ToListAsync(ct);

        return new RoutingOptions { ReportedDangerRoomIds = reported.ToHashSet() };
    }
}

/// <summary>
/// Kondygnacja wczytana z bazy. <see cref="Rooms"/> jest posortowane po numerze regionu,
/// więc pozycja na liście odpowiada numerowi wierzchołka w <see cref="Graph"/>.
/// </summary>
public sealed record LoadedFloor(FloorPlan Plan, IReadOnlyList<Room> Rooms, FloorGraph Graph);
