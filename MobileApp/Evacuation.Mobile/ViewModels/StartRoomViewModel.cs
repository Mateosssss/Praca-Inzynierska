using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Evacuation.Mobile.Models;
using Evacuation.Mobile.Services;

namespace Evacuation.Mobile.ViewModels;

public sealed partial class StartRoomViewModel(IEvacuationDataSource source, EvacuationSession session)
    : BaseViewModel
{
    private readonly List<RoomSummary> _allRooms = [];

    [ObservableProperty]
    public partial RoomSummary? SelectedRoom { get; set; }

    /// <summary>Czy aktywna jest zakładka ze wskazywaniem na rzucie zamiast listy sal.</summary>
    [ObservableProperty]
    public partial bool ShowPlan { get; set; }

    public ObservableCollection<RoomGroup> Groups { get; } = [];

    public string FloorLabel => session.Floor?.Label ?? "Piętro";

    public string? PlanImageUrl => source.GetPlanImageUrl(session.FloorPlanId);

    public bool HasPlanImage => PlanImageUrl is not null;

    public bool ShowList => !ShowPlan;

    public bool CanShowRoute => SelectedRoom is not null;

    /// <summary>Zaznaczone pomieszczenie jako pojedynczy punkt dla mapy.</summary>
    public IReadOnlyList<RouteStep> SelectionMarker => SelectedRoom is { } room
        ? [new RouteStep(1, room.Label, room.Id, room.RegionId, room.CentroidX, room.CentroidY)]
        : [];

    public string ShowRouteLabel => SelectedRoom is null
        ? "Wybierz pomieszczenie"
        : $"Wyznacz trasę z {SelectedRoom.LabelGenitive}";

    public Task LoadAsync() => RunAsync(async () =>
    {
        var rooms = await source.GetRoomsAsync(session.FloorPlanId);

        _allRooms.Clear();
        _allRooms.AddRange(rooms);

        Groups.Clear();
        foreach (var group in rooms.GroupBy(r => r.Group))
        {
            Groups.Add(new RoomGroup(group.Key, group));
        }

        OnPropertyChanged(nameof(FloorLabel));
        OnPropertyChanged(nameof(PlanImageUrl));
        OnPropertyChanged(nameof(HasPlanImage));
    });

    partial void OnSelectedRoomChanged(RoomSummary? value)
    {
        OnPropertyChanged(nameof(CanShowRoute));
        OnPropertyChanged(nameof(ShowRouteLabel));
        OnPropertyChanged(nameof(SelectionMarker));

        ShowRouteCommand.NotifyCanExecuteChanged();
    }

    partial void OnShowPlanChanged(bool value) => OnPropertyChanged(nameof(ShowList));

    [RelayCommand]
    private void ShowRoomList() => ShowPlan = false;

    [RelayCommand]
    private void ShowRoomPlan() => ShowPlan = true;

    /// <summary>
    /// Obsługa stuknięcia w rzut. O to, które pomieszczenie leży pod pikselem, pyta backend —
    /// tylko on zna mapę etykiet powstałą przy segmentacji.
    /// </summary>
    [RelayCommand]
    private Task PointTappedAsync(Point point) => RunAsync(async () =>
    {
        var roomId = await source.FindRoomAtAsync(
            session.FloorPlanId,
            (int)Math.Round(point.X),
            (int)Math.Round(point.Y));

        if (roomId is null)
        {
            // Ściana, drzwi albo miejsce poza rzutem — zostawiamy poprzedni wybór nietknięty.
            ErrorMessage = "W tym miejscu nie ma pomieszczenia.";
            return;
        }

        SelectedRoom = _allRooms.FirstOrDefault(room => room.Id == roomId);
    });

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
