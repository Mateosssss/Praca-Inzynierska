using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Services;

/// <summary>
/// Wybory użytkownika przenoszone między ekranami: budynek → piętro → pomieszczenie startowe.
/// Trzymamy je w singletonie zamiast serializować do parametrów trasy Shella — modele są
/// rekordami, a nie stringami, więc przekazywanie ich przez query string wymagałoby
/// sztucznego spłaszczania.
/// </summary>
public sealed class EvacuationSession
{
    public BuildingSummary? Building { get; set; }

    public FloorSummary? Floor { get; set; }

    public RoomSummary? StartRoom { get; set; }

    public EvacuationRoute? Route { get; set; }

    public DangerAlertResult? Alert { get; set; }

    public Guid BuildingId => Building?.Id ?? Guid.Empty;

    public int FloorNumber => Floor?.Number ?? 0;

    public void Reset()
    {
        Building = null;
        Floor = null;
        StartRoom = null;
        Route = null;
        Alert = null;
    }
}
