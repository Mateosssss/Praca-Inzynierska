using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Evacuation.Infrastructure.Persistence;

/// <summary>
/// Tworzy kontekst na potrzeby narzędzia <c>dotnet ef</c>.
/// </summary>
/// <remarks>
/// Polecenia w rodzaju <c>migrations add</c> potrzebują tylko modelu, a nie działającej bazy.
/// Bez tej fabryki narzędzie próbowałoby zbudować kontekst przez kontener DI aplikacji, a ten
/// oczekuje connection stringa wstrzykiwanego przez Aspire — generowanie migracji wymagałoby
/// więc uruchomionego Dockera i Postgresa. Podany niżej adres nie jest nigdy używany do połączenia;
/// dostarcza wyłącznie informacji, że dialektem jest PostgreSQL.
/// </remarks>
public sealed class EvacuationDbContextFactory : IDesignTimeDbContextFactory<EvacuationDbContext>
{
    public EvacuationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EvacuationDbContext>()
            .UseNpgsql("Host=localhost;Database=evacuationdb;Username=postgres")
            .Options;

        return new EvacuationDbContext(options);
    }
}
