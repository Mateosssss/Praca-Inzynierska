using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class BuildingsViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    public ObservableCollection<BuildingSummary> Buildings { get; } = [];

    /// <summary>
    /// Docelowo z <c>Geolocation.Default.GetLocationAsync</c>; na razie stała z makiety.
    /// </summary>
    public string LocationLabel => "53.0186 N · 18.6060 E — dokładność 8 m";

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
        var buildings = await source.GetNearbyBuildingsAsync();

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
}
