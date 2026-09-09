using Evacuation.Mobile.Views;

namespace Evacuation.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Trasy inne niż korzeń rejestrujemy ręcznie — Shell rozwiązuje strony przez DI.
        Routing.RegisterRoute(Routes.Floors, typeof(FloorsPage));
        Routing.RegisterRoute(Routes.Rooms, typeof(StartRoomPage));
        Routing.RegisterRoute(Routes.Route, typeof(RoutePage));
        Routing.RegisterRoute(Routes.Alarm, typeof(AlarmPage));
    }
}
