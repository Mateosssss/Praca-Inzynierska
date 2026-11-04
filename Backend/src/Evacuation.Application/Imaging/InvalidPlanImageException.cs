namespace Evacuation.Application.Imaging;

/// <summary>
/// Wgrany plik nie nadaje się na rzut piętra. Komunikat jest pisany pod administratora
/// budynku, nie pod programistę — to on zobaczy go w panelu po nieudanym wgraniu.
/// </summary>
public sealed class InvalidPlanImageException(string message) : Exception(message);
