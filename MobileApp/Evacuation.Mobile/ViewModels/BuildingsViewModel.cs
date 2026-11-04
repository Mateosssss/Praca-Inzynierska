using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class BuildingsViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    /// <summary>Pozycja zapasowa, gdy urządzenie nie udostępni własnej — centrum kampusu UMK.</summary>
    private const double FallbackLatitude = 53.0186;
    private const double FallbackLongitude = 18.6060;

    [ObservableProperty]
    public partial string LocationLabel { get; set; } = "Ustalanie położenia…";

    public ObservableCollection<BuildingSummary> Buildings { get; } = [];

    public async Task LoadAsync()
    {
        if (Buildings.Count > 0)
        {
            return;
        }

        await RefreshAsync();
    }

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(async () =>
    {
        var position = await ResolveLocationAsync();

        LocationLabel = position.Label;

        var buildings = await source.GetNearbyBuildingsAsync(position.Latitude, position.Longitude);

        Buildings.Clear();
        foreach (var building in buildings)
        {
            Buildings.Add(building);
        }
    });

    [RelayCommand]
    private async Task SelectBuildingAsync(BuildingSummary? building)
    {
        if (building is null)
        {
            return;
        }

        session.Reset();
        session.Building = building;

        await Shell.Current.GoToAsync(Routes.Floors);
    }

    /// <summary>
    /// Pobiera położenie urządzenia. Odmowa uprawnienia, wyłączony GPS albo brak odczytu
    /// nie przerywają działania — lista budynków pokazuje się wtedy dla pozycji zapasowej,
    /// zamiast zostawiać użytkownika z pustym ekranem.
    /// </summary>
    private static async Task<(double Latitude, double Longitude, string Label)> ResolveLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                var location = await Geolocation.Default.GetLocationAsync(request)
                    ?? await Geolocation.Default.GetLastKnownLocationAsync();

                if (location is not null)
                {
                    var accuracy = location.Accuracy is { } metres
                        ? $" — dokładność {metres:0} m"
                        : string.Empty;

                    return (
                        location.Latitude,
                        location.Longitude,
                        $"{location.Latitude:0.0000} N · {location.Longitude:0.0000} E{accuracy}");
                }
            }
        }
        catch (Exception)
        {
            // Urządzenie bez modułu GPS albo usługa niedostępna — schodzimy do pozycji zapasowej.
        }

        return (FallbackLatitude, FallbackLongitude, "Brak dostępu do GPS — pozycja domyślna");
    }
}
