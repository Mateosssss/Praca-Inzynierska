var builder = DistributedApplication.CreateBuilder(args);

// Kontener PostgreSQL. Hasło generowane jest przez Aspire i trzymane w user secrets
// projektu AppHost, więc nie trafia do repozytorium.
var postgres = builder.AddPostgres("postgres")
    // Wolumen danych — graf pomieszczeń i zastosowane migracje przeżywają restart AppHosta.
    .WithDataVolume("evacuation-pgdata")
    // Kontener nie jest ubijany po zamknięciu AppHosta; kolejne uruchomienia są szybsze.
    .WithLifetime(ContainerLifetime.Persistent)
    // pgAdmin do podglądu tabel Building / FloorPlan / Room / RoomEdge / DangerZone.
    .WithPgAdmin();

var evacuationDb = postgres.AddDatabase("evacuationdb");

builder.AddProject<Projects.Evacuation_Api>("api")
    // Connection string wstrzykiwany jako ConnectionStrings__evacuationdb.
    .WithReference(evacuationDb)
    // API startuje dopiero, gdy baza przyjmuje połączenia.
    .WaitFor(evacuationDb);

builder.Build().Run();
