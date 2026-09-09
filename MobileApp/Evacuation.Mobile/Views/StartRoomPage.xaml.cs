using Evacuation.Mobile.ViewModels;

namespace Evacuation.Mobile.Views;

public partial class StartRoomPage : ContentPage
{
    private readonly StartRoomViewModel _viewModel;

    public StartRoomPage(StartRoomViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
