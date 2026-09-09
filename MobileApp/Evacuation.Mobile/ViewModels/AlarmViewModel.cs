using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class AlarmViewModel(EvacuationSession session) : BaseViewModel
{
    [ObservableProperty]
    public partial DangerZoneAlert? Alert { get; set; }

    [ObservableProperty]
    public partial EvacuationRoute? Route { get; set; }

    /// <summary>Trasa sprzed zgłoszenia — pokazywana jako odniesienie „trasa poprzednia”.</summary>
    public EvacuationRoute? PreviousRoute { get; private set; }

    public bool HasPrevious => PreviousRoute is not null;

    public Task LoadAsync() => RunAsync(() =>
    {
        PreviousRoute = session.Route;
        Alert = session.Alert?.Alert;
        Route = session.Alert?.Route;

        OnPropertyChanged(nameof(PreviousRoute));
        OnPropertyChanged(nameof(HasPrevious));

        return Task.CompletedTask;
    });

    [RelayCommand]
    private async Task BackToStartAsync()
    {
        session.Reset();
        await Shell.Current.GoToAsync(Routes.Buildings);
    }
}
