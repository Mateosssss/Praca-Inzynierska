using Evacuation.Mobile.Services;
using Evacuation.Mobile.ViewModels;
using Evacuation.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace Evacuation.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Źródło danych. Podmiana na implementację HTTP nie dotknie ViewModeli.
        builder.Services.AddSingleton<IEvacuationDataSource, MockEvacuationDataSource>();
        builder.Services.AddSingleton<EvacuationSession>();

        builder.Services.AddTransient<BuildingsViewModel>();
        builder.Services.AddTransient<FloorsViewModel>();
        builder.Services.AddTransient<StartRoomViewModel>();
        builder.Services.AddTransient<RouteViewModel>();
        builder.Services.AddTransient<AlarmViewModel>();

        builder.Services.AddTransient<BuildingsPage>();
        builder.Services.AddTransient<FloorsPage>();
        builder.Services.AddTransient<StartRoomPage>();
        builder.Services.AddTransient<RoutePage>();
        builder.Services.AddTransient<AlarmPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
