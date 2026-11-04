using Evacuation.Api.Contracts;
using Evacuation.Domain;
using Evacuation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Api.Controllers;

[ApiController]
[Route("api/buildings")]
public sealed class BuildingsController(EvacuationDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateBuildingRequest request, CancellationToken ct)
    {
        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
        };

        db.Buildings.Add(building);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Floors), new { buildingId = building.Id }, building.Id);
    }

    /// <summary>Budynki w zasięgu podanych współrzędnych, od najbliższego.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NearbyBuildingResponse>>> Nearby(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusMeters = 1000,
        CancellationToken ct = default)
    {
        var (minLat, maxLat, minLon, maxLon) = Geo.BoundingBox(latitude, longitude, radiusMeters);

        // Prostokąt odsiewa w bazie, korzystając z indeksu na współrzędnych.
        // Dokładna odległość po wielkim kole liczona jest dopiero na tym, co zostało.
        var candidates = await db.Buildings
            .AsNoTracking()
            .Where(b => b.Latitude >= minLat && b.Latitude <= maxLat
                && b.Longitude >= minLon && b.Longitude <= maxLon)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Address,
                b.Latitude,
                b.Longitude,
                FloorCount = b.FloorPlans.Count,
                AlarmLevel = b.FloorPlans
                    .Where(p => p.Rooms.Any(r => r.DangerZones.Any(z => z.Status == DangerZoneStatus.Active)))
                    .Select(p => (int?)p.Level)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var result = candidates
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Address,
                b.FloorCount,
                b.AlarmLevel,
                Distance = Geo.DistanceMeters(latitude, longitude, b.Latitude, b.Longitude),
            })
            .Where(b => b.Distance <= radiusMeters)
            .OrderBy(b => b.Distance)
            .Select(b => new NearbyBuildingResponse(
                b.Id,
                b.Name,
                b.Address,
                (int)Math.Round(b.Distance),
                b.FloorCount,
                b.AlarmLevel is null ? null : $"Aktywny alarm · piętro {b.AlarmLevel}"))
            .ToList();

        return Ok(result);
    }

    [HttpGet("{buildingId:guid}/floors")]
    public async Task<ActionResult<IReadOnlyList<FloorSummaryResponse>>> Floors(
        Guid buildingId,
        CancellationToken ct)
    {
        if (!await db.Buildings.AnyAsync(b => b.Id == buildingId, ct))
        {
            return NotFound();
        }

        var floors = await db.FloorPlans
            .AsNoTracking()
            .Where(plan => plan.BuildingId == buildingId)
            .OrderByDescending(plan => plan.Level)
            .Select(plan => new FloorSummaryResponse(
                plan.Id,
                plan.Level,
                plan.Label,
                plan.Rooms.Count,
                plan.Rooms.Count(room => room.IsStaircase),
                plan.Rooms.Count(room => room.IsExit),
                plan.Rooms.Count(room =>
                    room.HasPaintedDanger
                    || room.DangerZones.Any(zone => zone.Status == DangerZoneStatus.Active)),
                plan.Status == FloorPlanStatus.Processed))
            .ToListAsync(ct);

        return Ok(floors);
    }
}
