using Evacuation.Domain;
using Microsoft.EntityFrameworkCore;

namespace Evacuation.Infrastructure.Persistence;

public class EvacuationDbContext(DbContextOptions<EvacuationDbContext> options) : DbContext(options)
{
    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<RoomEdge> RoomEdges => Set<RoomEdge>();

    public DbSet<DangerZone> DangerZones => Set<DangerZone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBuilding(modelBuilder);
        ConfigureFloorPlan(modelBuilder);
        ConfigureRoom(modelBuilder);
        ConfigureRoomEdge(modelBuilder);
        ConfigureDangerZone(modelBuilder);
    }

    private static void ConfigureBuilding(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(building => building.Id);

            entity.Property(building => building.Name).HasMaxLength(200).IsRequired();
            entity.Property(building => building.Address).HasMaxLength(300).IsRequired();

            // Wyszukiwanie budynków w pobliżu filtruje po prostokącie współrzędnych.
            // Do skali tego projektu indeks złożony wystarcza; przy tysiącach budynków
            // właściwym narzędziem byłby PostGIS z indeksem przestrzennym.
            entity.HasIndex(building => new { building.Latitude, building.Longitude });
        });

    private static void ConfigureFloorPlan(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<FloorPlan>(entity =>
        {
            entity.HasKey(plan => plan.Id);

            entity.Property(plan => plan.Label).HasMaxLength(100).IsRequired();
            entity.Property(plan => plan.ImagePng).IsRequired();

            entity.HasOne(plan => plan.Building)
                .WithMany(building => building.FloorPlans)
                .HasForeignKey(plan => plan.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Jeden budynek nie może mieć dwóch rzutów tej samej kondygnacji.
            entity.HasIndex(plan => new { plan.BuildingId, plan.Level }).IsUnique();
        });

    private static void ConfigureRoom(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(room => room.Id);

            entity.Property(room => room.Label).HasMaxLength(200);

            entity.HasOne(room => room.FloorPlan)
                .WithMany(plan => plan.Rooms)
                .HasForeignKey(room => room.FloorPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Numer regionu jest unikalny w obrębie rzutu — to on odtwarza indeksy grafu.
            entity.HasIndex(room => new { room.FloorPlanId, room.RegionId }).IsUnique();

            // Wyszukiwanie wyjść przy budowaniu tablicy ewakuacyjnej.
            entity.HasIndex(room => new { room.FloorPlanId, room.IsExit });
        });

    private static void ConfigureRoomEdge(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<RoomEdge>(entity =>
        {
            entity.HasKey(edge => edge.Id);

            entity.HasOne(edge => edge.FloorPlan)
                .WithMany(plan => plan.Edges)
                .HasForeignKey(edge => edge.FloorPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dwa klucze obce do tej samej tabeli. Gdyby oba kasowały kaskadowo, usunięcie rzutu
            // dawałoby wiele ścieżek kaskady do tego samego wiersza i PostgreSQL by to odrzucił.
            // Krawędzie i tak znikają razem z rzutem, więc tu kaskada jest zbędna.
            entity.HasOne(edge => edge.FromRoom)
                .WithMany()
                .HasForeignKey(edge => edge.FromRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(edge => edge.ToRoom)
                .WithMany()
                .HasForeignKey(edge => edge.ToRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Para pomieszczeń ma w obrębie rzutu co najwyżej jedno przejście —
            // przy kilku drzwiach zostaje najkrótsze.
            entity.HasIndex(edge => new { edge.FloorPlanId, edge.FromRoomId, edge.ToRoomId }).IsUnique();
        });

    private static void ConfigureDangerZone(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<DangerZone>(entity =>
        {
            entity.HasKey(zone => zone.Id);

            entity.Property(zone => zone.Kind).HasMaxLength(100).IsRequired();

            entity.HasOne(zone => zone.Room)
                .WithMany(room => room.DangerZones)
                .HasForeignKey(zone => zone.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wyznaczanie trasy pyta wyłącznie o zagrożenia aktywne.
            entity.HasIndex(zone => new { zone.RoomId, zone.Status });
        });
}
