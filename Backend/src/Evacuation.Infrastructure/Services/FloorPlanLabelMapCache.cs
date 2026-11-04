using Evacuation.Application.Imaging;
using Evacuation.Application.Segmentation;
using Evacuation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Evacuation.Infrastructure.Services;

/// <summary>
/// Udostępnia mapę etykiet rzutu, czyli odpowiedź na pytanie „który piksel należy do którego
/// pomieszczenia”. Potrzebna, gdy użytkownik wskazuje pomieszczenie dotknięciem planu.
/// </summary>
/// <remarks>
/// Mapy nie trzymamy w bazie — dla rzutu o dwóch milionach pikseli to osiem megabajtów na
/// kondygnację, a odtworzenie jej z zapisanego PNG kosztuje jeden przebieg segmentacji.
/// Wynik trafia do pamięci podręcznej, więc płaci za niego dopiero pierwsze dotknięcie planu,
/// a kolejne są natychmiastowe.
/// </remarks>
public sealed class FloorPlanLabelMapCache(
    EvacuationDbContext db,
    IPlanImageDecoder decoder,
    IMemoryCache cache)
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    public async Task<SegmentationResult?> GetAsync(Guid floorPlanId, CancellationToken ct = default)
    {
        if (cache.TryGetValue<SegmentationResult>(Key(floorPlanId), out var cached) && cached is not null)
        {
            return cached;
        }

        var png = await db.FloorPlans
            .AsNoTracking()
            .Where(plan => plan.Id == floorPlanId)
            .Select(plan => plan.ImagePng)
            .FirstOrDefaultAsync(ct);

        if (png is null)
        {
            return null;
        }

        var segmentation = RoomSegmenter.Segment(decoder.Decode(new MemoryStream(png)));

        cache.Set(Key(floorPlanId), segmentation, new MemoryCacheEntryOptions
        {
            SlidingExpiration = Lifetime,
            // Mapa etykiet bywa duża; bez tego kilka kondygnacji naraz potrafiłoby
            // rozdąć pamięć procesu.
            Size = 1,
        });

        return segmentation;
    }

    /// <summary>Unieważnia mapę po ponownym wgraniu rzutu.</summary>
    public void Invalidate(Guid floorPlanId) => cache.Remove(Key(floorPlanId));

    private static string Key(Guid floorPlanId) => $"labelmap:{floorPlanId}";
}
