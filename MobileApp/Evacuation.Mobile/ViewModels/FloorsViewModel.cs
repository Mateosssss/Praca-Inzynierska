using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class FloorsViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    public ObservableCollection<FloorSummary> Floors { get; } = [];

    public string BuildingName => session.Building?.Name ?? "Budynek";

    public string Note =>
        "Piętra połączone przez pokoje z flagą IsStaircase — trasa może prowadzić między kondygnacjami.";

    public Task LoadAsync() => RunAsync(async () =>
    {
        var floors = await source.GetFloorsAsync(session.BuildingId);

        Floors.Clear();
        foreach (var floor in floors)
        {
            Floors.Add(floor);
        }

        OnPropertyChanged(nameof(BuildingName));
    });

    [RelayCommand]
    private async Task SelectFloorAsync(FloorSummary? floor)
    {
        // Piętro bez wgranego rzutu nie ma grafu, więc nie ma z czego liczyć trasy.
        if (floor is null || !floor.HasPlan)
        {
            return;
        }

        session.Floor = floor;
        await Shell.Current.GoToAsync(Routes.Rooms);
    }
}
