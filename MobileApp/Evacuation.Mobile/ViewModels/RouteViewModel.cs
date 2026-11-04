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

    /// <summary>Numer przystanku, na którym stoi użytkownik. Liczony od zera.</summary>
    [ObservableProperty]
    public partial int CurrentStepIndex { get; set; }

    public string? PlanImageUrl => source.GetPlanImageUrl(session.FloorPlanId);

    public bool HasRoute => Route is not null;

    public bool HasPlanImage => PlanImageUrl is not null;

    public IReadOnlyList<RouteStep> Steps => Route?.Steps ?? [];

    public string StepCounter => Route is null
        ? string.Empty
        : $"Krok {CurrentStepIndex + 1} z {Route.Steps.Count}";

    public string CurrentInstruction =>
        CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count
            ? Steps[CurrentStepIndex].Instruction
            : string.Empty;

    public bool CanGoPrevious => CurrentStepIndex > 0;

    public bool CanGoNext => CurrentStepIndex < Steps.Count - 1;

    public string Title => $"{session.Floor?.Label ?? "Piętro"} — trasa ewakuacji";

    public string Subtitle => session.StartRoom is { } room ? $"z {room.LabelGenitive}" : string.Empty;

    partial void OnRouteChanged(EvacuationRoute? value)
    {
        CurrentStepIndex = 0;

        OnPropertyChanged(nameof(HasRoute));
        OnPropertyChanged(nameof(Steps));
        NotifyStepChanged();
    }

    partial void OnCurrentStepIndexChanged(int value) => NotifyStepChanged();

    private void NotifyStepChanged()
    {
        OnPropertyChanged(nameof(StepCounter));
        OnPropertyChanged(nameof(CurrentInstruction));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));

        PreviousStepCommand.NotifyCanExecuteChanged();
        NextStepCommand.NotifyCanExecuteChanged();
    }

    public Task LoadAsync() => RunAsync(async () =>
    {
        Route = await source.GetRouteAsync(session.FloorPlanId, session.StartRoomId);

        session.Route = Route;

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(PlanImageUrl));
        OnPropertyChanged(nameof(HasPlanImage));
    });

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextStep() => CurrentStepIndex++;

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousStep() => CurrentStepIndex--;

    [RelayCommand]
    private Task ReportDangerAsync() => RunAsync(async () =>
    {
        session.Alert = await source.ReportDangerZoneAsync(
            session.FloorPlanId,
            session.StartRoomId,
            kind: "pożar");

        await Shell.Current.GoToAsync(Routes.Alarm);
    });
}
