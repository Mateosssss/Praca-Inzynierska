namespace Evacuation.Application.Graph;

/// <summary>
/// Usterka wykryta w rzucie przy budowaniu grafu. Współrzędne wskazują miejsce na planie,
/// dzięki czemu panel administratora może pokazać administratorowi, gdzie dokładnie leży problem,
/// zamiast odrzucać cały rzut bez wyjaśnienia.
/// </summary>
public sealed record PlanWarning(string Message, int X, int Y);
