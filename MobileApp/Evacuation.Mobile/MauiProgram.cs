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

        builder.Services.AddSingleton<EvacuationSession>();

        // Przełącznik źródła danych. Ustaw na true, żeby pracować nad samym interfejsem
        // bez uruchamiania backendu — ekrany wypełnią się wtedy danymi z makiet.
        var useMockData = false;

        if (useMockData)
        {
            builder.Services.AddSingleton<IEvacuationDataSource, MockEvacuationDataSource>();
        }
        else
        {
            var api = new ApiOptions();

            builder.Services.AddSingleton(api);

            // Jedna instancja na całą aplikację. W aplikacji mobilnej, rozmawiającej z jednym
            // serwerem, nie ma powodu tworzyć ich więcej ani obawiać się zmian DNS.
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(api.BaseAddress.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(15),
            });

            builder.Services.AddSingleton<IEvacuationDataSource, ApiEvacuationDataSource>();
        }

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
