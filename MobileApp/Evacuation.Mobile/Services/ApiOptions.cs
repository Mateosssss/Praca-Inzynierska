namespace Evacuation.Mobile.Services;

/// <summary>Adres backendu.</summary>
/// <remarks>
/// <para>
/// Telefon nie widzi <c>localhost</c> komputera deweloperskiego — to jego własny localhost.
/// Właściwy adres zależy od tego, na czym uruchamiasz aplikację:
/// </para>
/// <list type="bullet">
/// <item>emulator Androida — host widoczny jest pod <c>10.0.2.2</c></item>
/// <item>fizyczny telefon — adres komputera w sieci lokalnej, np. <c>192.168.0.12</c>;
/// komputer i telefon muszą być w tej samej sieci, a zapora musi przepuścić port</item>
/// </list>
/// <para>
/// Na czas developmentu używamy zwykłego HTTP. Po HTTPS telefon odrzuciłby połączenie,
/// bo nie ufa certyfikatowi deweloperskiemu komputera.
/// </para>
/// </remarks>
public sealed class ApiOptions
{
    /// <summary>Adres bazowy API wraz ze schematem i portem.</summary>
    public string BaseAddress { get; init; } = "http://192.168.100.3:5097";

    /// <summary>Promień wyszukiwania budynków w metrach.</summary>
    public double SearchRadiusMeters { get; init; } = 1000;
}
