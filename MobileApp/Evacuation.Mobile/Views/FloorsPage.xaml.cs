using Evacuation.Mobile.ViewModels;

namespace Evacuation.Mobile.Views;

public partial class FloorsPage : ContentPage
{
    private readonly FloorsViewModel _viewModel;

    public FloorsPage(FloorsViewModel viewModel)
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
