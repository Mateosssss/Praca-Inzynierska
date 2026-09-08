using System.Reflection;

namespace Evacuation.Application.Tests;

/// <summary>
/// Pilnuje granicy warstw opisanej w README: algorytmy z warstwy Application mają być
/// testowalne na syntetycznych bitmapach, bez bazy danych i bez hostingu HTTP.
/// Test przestanie przechodzić, gdy ktoś doda do Application referencję na EF Core,
/// ASP.NET Core albo sterownik Postgresa.
/// </summary>
public class ArchitectureTests
{
    private static readonly string[] ForbiddenPrefixes =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql",
    ];

    [Fact]
    public void Application_nie_zalezy_od_EF_Core_ani_HTTP()
    {
        var application = Assembly.Load("Evacuation.Application");

        var naruszenia = application.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => ForbiddenPrefixes.Any(
                prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToArray();

        Assert.True(
            naruszenia.Length == 0,
            $"Warstwa Application nie powinna zależeć od: {string.Join(", ", naruszenia)}");
    }

    [Fact]
    public void Domain_nie_ma_zaleznosci_zewnetrznych()
    {
        var domain = Assembly.Load("Evacuation.Domain");

        var naruszenia = domain.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => !name.StartsWith("System", StringComparison.Ordinal)
                        && !name.Equals("netstandard", StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            naruszenia.Length == 0,
            $"Warstwa Domain powinna być czystym C#, a zależy od: {string.Join(", ", naruszenia)}");
    }
}
