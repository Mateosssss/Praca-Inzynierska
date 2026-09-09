using Evacuation.Mobile.ViewModels;

namespace Evacuation.Mobile.Views;

public partial class BuildingsPage : ContentPage
{
    private readonly BuildingsViewModel _viewModel;

    public BuildingsPage(BuildingsViewModel viewModel)
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
