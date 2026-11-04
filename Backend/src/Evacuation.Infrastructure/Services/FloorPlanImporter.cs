using Evacuation.Application.Graph;
using Evacuation.Application.Imaging;
using Evacuation.Application.Segmentation;
using Evacuation.Domain;
using Evacuation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Infrastructure.Services;

/// <summary>
/// Przyjmuje wgrany rzut, przepuszcza go przez cały pipeline i zapisuje wynikowy graf.
/// </summary>
/// <remarks>
/// Segmentacja biegnie raz, przy wgrywaniu. Przy wyznaczaniu trasy czytany jest już gotowy graf
/// z bazy — obraz nie jest przetwarzany ponownie przy każdym zapytaniu użytkownika.
/// </remarks>
public sealed class FloorPlanImporter(EvacuationDbContext db, IPlanImageDecoder decoder)
{
    public async Task<FloorPlanImportResult> ImportAsync(
        Guid buildingId,
        int level,
        string label,
        double pixelsPerMeter,
        byte[] png,
        CancellationToken ct = default)
    {
        if (pixelsPerMeter <= 0)
        {
            throw new InvalidPlanImageException("Skala rzutu musi być dodatnia.");
        }

        var plan = decoder.Decode(new MemoryStream(png));
        var segmentation = RoomSegmenter.Segment(plan);
        var graph = GraphBuilder.Build(plan, segmentation);

        // Ponowne wgranie tej samej kondygnacji zastępuje poprzedni rzut. Kaskada usuwa razem
        // z nim pomieszczenia i krawędzie, więc stary graf nie zostaje w bazie obok nowego.
        var existing = await db.FloorPlans
            .FirstOrDefaultAsync(p => p.BuildingId == buildingId && p.Level == level, ct);

        if (existing is not null)
        {
            db.FloorPlans.Remove(existing);
        }

        var entity = new FloorPlan
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Level = level,
            Label = label,
            ImagePng = png,
            WidthPx = plan.Width,
            HeightPx = plan.Height,
            PixelsPerMeter = pixelsPerMeter,
            Status = FloorPlanStatus.Processed,
            UploadedAt = DateTimeOffset.UtcNow,
        };

        // Indeks regionu to pozycja na liście, więc tablica wystarcza za słownik.
        var roomIds = new Guid[graph.Rooms.Count];

        foreach (var region in graph.Rooms)
        {
            roomIds[region.Id] = Guid.NewGuid();

            entity.Rooms.Add(new Room
            {
                Id = roomIds[region.Id],
                FloorPlanId = entity.Id,
                RegionId = region.Id,
                Label = null,
                CentroidX = region.CentroidX,
                CentroidY = region.CentroidY,
                PixelCount = region.PixelCount,
                IsExit = region.HasExit,
                IsStaircase = region.HasStairs,
                IsStart = region.HasStart,
                HasPaintedDanger = region.HasDanger,
            });
        }

        foreach (var edge in graph.Edges)
        {
            entity.Edges.Add(new Evacuation.Domain.RoomEdge
            {
                Id = Guid.NewGuid(),
                FloorPlanId = entity.Id,
                FromRoomId = roomIds[edge.FromRoomId],
                ToRoomId = roomIds[edge.ToRoomId],
                WeightPixels = edge.WeightPixels,
            });
        }

        db.FloorPlans.Add(entity);
        await db.SaveChangesAsync(ct);

        return new FloorPlanImportResult(entity, graph, segmentation);
    }
}

/// <summary>
/// Zapisany rzut wraz z tym, co wyszło z przetwarzania — panel administratora pokazuje
/// liczbę regionów, krawędzi i wykryte usterki, zanim administrator zatwierdzi wynik.
/// </summary>
public sealed record FloorPlanImportResult(
    FloorPlan FloorPlan,
    FloorGraph Graph,
    SegmentationResult Segmentation);
