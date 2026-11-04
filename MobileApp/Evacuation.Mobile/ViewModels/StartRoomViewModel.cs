using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class StartRoomViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    [ObservableProperty]
    public partial RoomSummary? SelectedRoom { get; set; }

    public ObservableCollection<RoomGroup> Groups { get; } = [];

    public string FloorLabel => session.Floor?.Label ?? "Piętro";

    public bool CanShowRoute => SelectedRoom is not null;

    /// <summary>„Wyznacz trasę z sali 118” — tekst przycisku zależny od wyboru.</summary>
    public string ShowRouteLabel => SelectedRoom is null
        ? "Wybierz pomieszczenie"
        : $"Wyznacz trasę z {SelectedRoom.LabelGenitive}";

    public Task LoadAsync() => RunAsync(async () =>
    {
        var rooms = await source.GetRoomsAsync(session.FloorPlanId);

        Groups.Clear();
        foreach (var group in rooms.GroupBy(r => r.Group))
        {
            Groups.Add(new RoomGroup(group.Key, group));
        }

        OnPropertyChanged(nameof(FloorLabel));
    });

    partial void OnSelectedRoomChanged(RoomSummary? value)
    {
        OnPropertyChanged(nameof(CanShowRoute));
        OnPropertyChanged(nameof(ShowRouteLabel));
        ShowRouteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanShowRoute))]
    private async Task ShowRouteAsync()
    {
        if (SelectedRoom is null)
        {
            return;
        }

        session.StartRoom = SelectedRoom;
        await Shell.Current.GoToAsync(Routes.Route);
    }

}
