using Evacuation.Mobile.ViewModels;

namespace Evacuation.Mobile.Views;

public partial class RoutePage : ContentPage
{
    private readonly RouteViewModel _viewModel;

    public RoutePage(RouteViewModel viewModel)
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
