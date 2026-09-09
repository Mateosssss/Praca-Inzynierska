using Evacuation.Mobile.ViewModels;

namespace Evacuation.Mobile.Views;

public partial class AlarmPage : ContentPage
{
    private readonly AlarmViewModel _viewModel;

    public AlarmPage(AlarmViewModel viewModel)
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
