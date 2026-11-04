using Evacuation.Api.Contracts;
using Evacuation.Application.Imaging;
using Evacuation.Application.Routing;
using Evacuation.Domain;
using Evacuation.Infrastructure.Persistence;
using Evacuation.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Api.Controllers;

[ApiController]
public sealed class FloorPlansController(
    EvacuationDbContext db,
    FloorPlanImporter importer,
    FloorGraphLoader loader) : ControllerBase
{
    /// <summary>Wgranie rzutu kondygnacji. Przetwarza obraz i zapisuje wyliczony graf.</summary>
    [HttpPost("api/buildings/{buildingId:guid}/floors")]
    [RequestSizeLimit(32 * 1024 * 1024)]
    public async Task<ActionResult<FloorPlanImportResponse>> Upload(
        Guid buildingId,
        [FromForm] UploadFloorPlanRequest request,
        CancellationToken ct)
    {
        if (!await db.Buildings.AnyAsync(b => b.Id == buildingId, ct))
        {
            return NotFound();
        }

        if (request.Image is null || request.Image.Length == 0)
        {
            return BadRequest(new ProblemDetails { Title = "Nie przesłano pliku rzutu." });
        }

        using var buffer = new MemoryStream();
        await request.Image.CopyToAsync(buffer, ct);

        FloorPlanImportResult result;

        try
        {
            result = await importer.ImportAsync(
                buildingId,
                request.Level,
                request.Label,
                request.PixelsPerMeter,
                buffer.ToArray(),
                ct);
        }
        catch (InvalidPlanImageException exception)
        {
            // Komunikat jest pisany pod administratora budynku i trafia wprost do panelu.
            return BadRequest(new ProblemDetails
            {
                Title = "Rzut nie przeszedł walidacji.",
                Detail = exception.Message,
            });
        }

        var table = EvacuationRouter.BuildTable(result.Graph);
        var withoutExit = table.RoomsWithoutExit();

        return Ok(new FloorPlanImportResponse(
            result.FloorPlan.Id,
            result.FloorPlan.Level,
            result.FloorPlan.Label,
            result.FloorPlan.WidthPx,
            result.FloorPlan.HeightPx,
            result.Graph.Rooms.Count,
            result.Graph.Edges.Count,
            result.Graph.Rooms.Count(room => room.HasExit),
            result.Graph.Rooms.Count(room => room.HasStairs),
            withoutExit.Count == 0,
            withoutExit,
            [.. result.Graph.Warnings.Select(w => new PlanWarningResponse(w.Message, w.X, w.Y))]));
    }

    /// <summary>Oryginalny plik rzutu — aplikacja mobilna rysuje na nim trasę.</summary>
    [HttpGet("api/floors/{floorPlanId:guid}/image")]
    public async Task<IActionResult> Image(Guid floorPlanId, CancellationToken ct)
    {
        var image = await db.FloorPlans
            .AsNoTracking()
            .Where(plan => plan.Id == floorPlanId)
            .Select(plan => plan.ImagePng)
            .FirstOrDefaultAsync(ct);

        return image is null ? NotFound() : File(image, "image/png");
    }

    [HttpGet("api/floors/{floorPlanId:guid}/rooms")]
    public async Task<ActionResult<IReadOnlyList<RoomSummaryResponse>>> Rooms(
        Guid floorPlanId,
        CancellationToken ct)
    {
        var plan = await db.FloorPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == floorPlanId, ct);

        if (plan is null)
        {
            return NotFound();
        }

        var rooms = await db.Rooms
            .AsNoTracking()
            .Where(room => room.FloorPlanId == floorPlanId)
            .OrderBy(room => room.RegionId)
            .Select(room => new
            {
                room.Id,
                room.RegionId,
                room.Label,
                room.PixelCount,
                room.CentroidX,
                room.CentroidY,
                room.IsExit,
                room.IsStaircase,
                HasActiveDanger = room.HasPaintedDanger
                    || room.DangerZones.Any(zone => zone.Status == DangerZoneStatus.Active),
            })
            .ToListAsync(ct);

        var squareMetersPerPixel = 1 / (plan.PixelsPerMeter * plan.PixelsPerMeter);

        var response = rooms
            .Select(room => new RoomSummaryResponse(
                room.Id,
                room.RegionId,
                room.Label ?? $"Pomieszczenie {room.RegionId}",
                $"region #{room.RegionId} · {room.PixelCount * squareMetersPerPixel:0.#} m²",
                room.PixelCount * squareMetersPerPixel,
                room.CentroidX,
                room.CentroidY,
                room.IsExit,
                room.IsStaircase,
                room.HasActiveDanger))
            .ToList();

        return Ok(response);
    }

    /// <summary>Trasa ewakuacji z podanego pomieszczenia do najbliższego wyjścia.</summary>
    [HttpGet("api/floors/{floorPlanId:guid}/route")]
    public async Task<ActionResult<RouteResponse>> Route(
        Guid floorPlanId,
        [FromQuery] Guid fromRoomId,
        CancellationToken ct)
    {
        var floor = await loader.LoadAsync(floorPlanId, ct);

        if (floor is null)
        {
            return NotFound();
        }

        var start = floor.Rooms.FirstOrDefault(room => room.Id == fromRoomId);

        if (start is null)
        {
            return NotFound(new ProblemDetails { Title = "Pomieszczenie nie należy do tej kondygnacji." });
        }

        var options = await loader.LoadRoutingOptionsAsync(floorPlanId, ct);
        var route = EvacuationRouter.FindRoute(floor.Graph, start.RegionId, options);

        if (route is null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Brak drogi do wyjścia.",
                Detail = "To pomieszczenie nie ma połączenia z żadnym wyjściem ewakuacyjnym. "
                    + "Najpewniej na rzucie brakuje drzwi.",
            });
        }

        // Numer regionu jest pozycją na liście, więc indeksowanie jest bezpieczne.
        var exitRoom = floor.Rooms[route.ExitRoomId];

        var steps = route.RoomIds
            .Select((regionId, index) =>
            {
                var room = floor.Rooms[regionId];

                return new RouteStepResponse(
                    index + 1,
                    room.Id,
                    room.RegionId,
                    room.Label ?? $"Pomieszczenie {room.RegionId}",
                    room.CentroidX,
                    room.CentroidY);
            })
            .ToList();

        return Ok(new RouteResponse(
            exitRoom.Id,
            exitRoom.Label ?? $"Pomieszczenie {exitRoom.RegionId}",
            route.LengthPixels / floor.Plan.PixelsPerMeter,
            route.SegmentCount,
            route.PassesThroughDanger,
            steps));
    }
}
