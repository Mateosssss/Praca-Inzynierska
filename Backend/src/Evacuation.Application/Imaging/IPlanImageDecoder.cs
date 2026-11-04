namespace Evacuation.Application.Imaging;

/// <summary>
/// Zamienia wgrany plik rzutu na <see cref="Plan"/>.
/// </summary>
/// <remarks>
/// Interfejs mieszka w warstwie algorytmów, a implementacja w <c>Evacuation.Infrastructure</c>.
/// Dzięki temu <c>Application</c> nie zna żadnej biblioteki graficznej i pozostaje testowalna
/// na planszach budowanych w kodzie — a podmiana biblioteki dekodującej nie dotyka algorytmów.
/// </remarks>
public interface IPlanImageDecoder
{
    /// <summary>
    /// Dekoduje obraz rzutu. Strumień musi zawierać PNG — format stratny unieważniłby
    /// dokładne porównywanie kolorów, na którym opiera się cały kontrakt.
    /// </summary>
    /// <exception cref="InvalidPlanImageException">
    /// Gdy dane nie są obrazem albo nie są w formacie PNG.
    /// </exception>
    Plan Decode(Stream image);
}
