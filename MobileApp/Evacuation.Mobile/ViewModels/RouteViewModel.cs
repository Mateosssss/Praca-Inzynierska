using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class RouteViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    [ObservableProperty]
    public partial EvacuationRoute? Route { get; set; }

    public bool HasRoute => Route is not null;

    partial void OnRouteChanged(EvacuationRoute? value) => OnPropertyChanged(nameof(HasRoute));

    public string Title => $"{session.Floor?.Label ?? "Piętro"} — trasa ewakuacji";

    public string Subtitle => session.StartRoom is { } room ? $"z {room.Label}" : string.Empty;

    public Task LoadAsync() => RunAsync(async () =>
    {
        Route = await source.GetRouteAsync(
            session.BuildingId,
            session.FloorNumber,
            session.StartRoom?.RegionId ?? 0);

        session.Route = Route;

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
    });

    [RelayCommand]
    private Task ReportDangerAsync() => RunAsync(async () =>
    {
        session.Alert = await source.ReportDangerZoneAsync(
            session.BuildingId,
            session.FloorNumber,
            session.StartRoom?.RegionId ?? 0);

        await Shell.Current.GoToAsync(Routes.Alarm);
    });

    [RelayCommand]
    private Task GuideAsync() =>
        // Nawigacja krok po kroku to osobny punkt roadmapy; na razie potwierdzamy akcję.
        Shell.Current.DisplayAlertAsync(
            "Prowadzenie",
            "Nawigacja krok po kroku nie jest jeszcze zaimplementowana.",
            "OK");
}
