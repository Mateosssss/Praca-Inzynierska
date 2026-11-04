using Evacuation.Api.Contracts;
using Evacuation.Domain;
using Evacuation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Api.Controllers;

[ApiController]
public sealed class DangerZonesController(EvacuationDbContext db) : ControllerBase
{
    /// <summary>
    /// Zgłoszenie zagrożenia w pomieszczeniu. Nie zmienia zapisanego grafu — graf opisuje budynek,
    /// a zagrożenie jest chwilowe i doliczane dopiero przy wyznaczaniu trasy.
    /// </summary>
    [HttpPost("api/rooms/{roomId:guid}/danger")]
    public async Task<ActionResult<DangerZoneResponse>> Report(
        Guid roomId,
        ReportDangerRequest request,
        CancellationToken ct)
    {
        if (!await db.Rooms.AnyAsync(room => room.Id == roomId, ct))
        {
            return NotFound();
        }

        var zone = new DangerZone
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            Kind = string.IsNullOrWhiteSpace(request.Kind) ? "nieokreślone" : request.Kind,
            ReportedAt = DateTimeOffset.UtcNow,
            Status = DangerZoneStatus.Active,
        };

        db.DangerZones.Add(zone);
        await db.SaveChangesAsync(ct);

        return Ok(new DangerZoneResponse(zone.Id, zone.RoomId, zone.Kind, zone.ReportedAt, zone.Status.ToString()));
    }

    [HttpPost("api/danger/{dangerZoneId:guid}/clear")]
    public async Task<IActionResult> Clear(Guid dangerZoneId, CancellationToken ct)
    {
        var zone = await db.DangerZones.FirstOrDefaultAsync(z => z.Id == dangerZoneId, ct);

        if (zone is null)
        {
            return NotFound();
        }

        zone.Status = DangerZoneStatus.Cleared;
        zone.ClearedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>Aktywne zagrożenia na kondygnacji.</summary>
    [HttpGet("api/floors/{floorPlanId:guid}/danger")]
    public async Task<ActionResult<IReadOnlyList<DangerZoneResponse>>> Active(
        Guid floorPlanId,
        CancellationToken ct)
    {
        var zones = await db.DangerZones
            .AsNoTracking()
            .Where(zone => zone.Status == DangerZoneStatus.Active
                && zone.Room != null
                && zone.Room.FloorPlanId == floorPlanId)
            .OrderByDescending(zone => zone.ReportedAt)
            .Select(zone => new DangerZoneResponse(
                zone.Id,
                zone.RoomId,
                zone.Kind,
                zone.ReportedAt,
                zone.Status.ToString()))
            .ToListAsync(ct);

        return Ok(zones);
    }
}
