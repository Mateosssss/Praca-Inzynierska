using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Pobiera dane z Evacuation.Api i tłumaczy je na modele ekranów.
/// </summary>
public sealed class ApiEvacuationDataSource(HttpClient http, ApiOptions options) : IEvacuationDataSource
{
    /// <summary>Przeciętne tempo marszu podczas ewakuacji, w metrach na sekundę.</summary>
    private const double WalkingSpeed = 1.25;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<BuildingSummary>> GetNearbyBuildingsAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        var query = string.Create(
            CultureInfo.InvariantCulture,
            $"api/buildings?latitude={latitude}&longitude={longitude}&radiusMeters={options.SearchRadiusMeters}");

        var buildings = await GetAsync<List<NearbyBuildingDto>>(query, ct);

        return buildings
            .Select(b => new BuildingSummary(b.Id, b.Name, b.Address, b.DistanceMeters, b.FloorCount, b.AlarmNote))
            .ToList();
    }

    public async Task<IReadOnlyList<FloorSummary>> GetFloorsAsync(Guid buildingId, CancellationToken ct = default)
    {
        var floors = await GetAsync<List<FloorSummaryDto>>($"api/buildings/{buildingId}/floors", ct);

        return floors
            .Select(f => new FloorSummary(
                f.Id,
                f.Level,
                f.Label,
                f.RoomCount,
                f.StaircaseCount,
                f.ExitCount,
                f.DangerZoneCount,
                f.HasPlan))
            .ToList();
    }

    public async Task<IReadOnlyList<RoomSummary>> GetRoomsAsync(Guid floorPlanId, CancellationToken ct = default)
    {
        var rooms = await GetAsync<List<RoomSummaryDto>>($"api/floors/{floorPlanId}/rooms", ct);

        return rooms.Select(ToRoom).ToList();
    }

    public async Task<EvacuationRoute> GetRouteAsync(
        Guid floorPlanId,
        Guid startRoomId,
        CancellationToken ct = default)
    {
        var route = await GetAsync<RouteDto>($"api/floors/{floorPlanId}/route?fromRoomId={startRoomId}", ct);

        return ToRoute(route, headline: "Najbliższe wyjście", previous: null);
    }

    public async Task<DangerAlertResult> ReportDangerZoneAsync(
        Guid floorPlanId,
        Guid roomId,
        string kind,
        CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"api/rooms/{roomId}/danger", new { kind }, Json, ct);

        response.EnsureSuccessStatusCode();

        var zone = await response.Content.ReadFromJsonAsync<DangerZoneDto>(Json, ct)
            ?? throw new EvacuationApiException("Serwer nie potwierdził zgłoszenia zagrożenia.");

        // Trasa przelicza się po stronie serwera — pytamy o nią ponownie, żeby zobaczyć skutek.
        var recalculated = await GetAsync<RouteDto>($"api/floors/{floorPlanId}/route?fromRoomId={roomId}", ct);

        var alert = new DangerZoneAlert(
            Title: "Alarm",
            Description: "Zgłoszono strefę zagrożenia",
            Kind: zone.Kind,
            ReportedAt: TimeOnly.FromTimeSpan(zone.ReportedAt.ToLocalTime().TimeOfDay),
            CostMultiplier: "×25 na krawędziach strefy",
            RecalculationStats: $"{recalculated.Steps.Count} wierzchołków na trasie");

        return new DangerAlertResult(alert, ToRoute(recalculated, "Nowa trasa", previous: null));
    }

    public async Task<Guid?> FindRoomAtAsync(
        Guid floorPlanId,
        int x,
        int y,
        CancellationToken ct = default)
    {
        var hit = await GetAsync<RoomAtDto>($"api/floors/{floorPlanId}/room-at?x={x}&y={y}", ct);

        return hit.RoomId;
    }

    public string GetPlanImageUrl(Guid floorPlanId) =>
        $"{options.BaseAddress.TrimEnd('/')}/api/floors/{floorPlanId}/image";

    private static RoomSummary ToRoom(RoomSummaryDto dto) => new(
        dto.Id,
        dto.RegionId,
        dto.Label,
        // Rzut nie niesie informacji o skrzydłach budynku, więc grupujemy po roli pomieszczenia.
        dto.IsStaircase ? "Ciągi komunikacyjne" : "Pomieszczenia",
        dto.Detail,
        dto.IsExit,
        dto.IsStaircase,
        dto.HasActiveDanger,
        dto.CentroidX,
        dto.CentroidY);

    private static EvacuationRoute ToRoute(RouteDto dto, string headline, double? previous)
    {
        var seconds = (int)Math.Round(dto.TotalMeters / WalkingSpeed);

        var steps = dto.Steps
            .Select(step => new RouteStep(
                step.Ordinal,
                step.Label,
                step.RoomId,
                step.RegionId,
                step.CentroidX,
                step.CentroidY))
            .ToList();

        var summary = $"{dto.ExitLabel} · {dto.SegmentCount} {Odcinki(dto.SegmentCount)} · ok. {seconds} s marszu";

        var comparison = previous is null
            ? null
            : $"{dto.TotalMeters - previous.Value:+0;-0} m względem poprzedniej";

        return new EvacuationRoute(
            headline,
            (int)Math.Round(dto.TotalMeters),
            summary,
            steps,
            comparison);
    }

    private static string Odcinki(int n) => n switch
    {
        1 => "odcinek",
        >= 2 and <= 4 => "odcinki",
        _ => "odcinków",
    };

    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            var response = await http.GetAsync(path, ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new EvacuationApiException(await DescribeAsync(response, ct));
            }

            return await response.Content.ReadFromJsonAsync<T>(Json, ct)
                ?? throw new EvacuationApiException("Serwer zwrócił pustą odpowiedź.");
        }
        catch (HttpRequestException exception)
        {
            throw new EvacuationApiException(
                $"Brak połączenia z serwerem pod adresem {options.BaseAddress}. "
                + "Sprawdź, czy backend działa i czy telefon jest w tej samej sieci.",
                exception);
        }
        catch (TaskCanceledException exception) when (!ct.IsCancellationRequested)
        {
            throw new EvacuationApiException("Serwer nie odpowiedział na czas.", exception);
        }
    }

    /// <summary>
    /// Wyciąga z odpowiedzi komunikat przeznaczony dla użytkownika. API zwraca błędy
    /// w formacie ProblemDetails, w którym sedno siedzi w polu <c>detail</c>.
    /// </summary>
    private static async Task<string> DescribeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json, ct);

            if (problem.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } text)
            {
                return text;
            }

            if (problem.TryGetProperty("title", out var title) && title.GetString() is { Length: > 0 } heading)
            {
                return heading;
            }
        }
        catch (JsonException)
        {
            // Odpowiedź nie była ProblemDetails — zostaje sam kod stanu.
        }

        return $"Serwer odpowiedział kodem {(int)response.StatusCode}.";
    }
}

public sealed class EvacuationApiException(string message, Exception? inner = null)
    : Exception(message, inner);
