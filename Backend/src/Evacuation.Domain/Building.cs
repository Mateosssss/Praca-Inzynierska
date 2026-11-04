namespace Evacuation.Domain;

/// <summary>
/// Budynek objęty systemem. Współrzędne służą do wyszukiwania po GPS — aplikacja mobilna
/// pyta „jakie budynki są w pobliżu”, zanim użytkownik wybierze piętro.
/// </summary>
public class Building
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public ICollection<FloorPlan> FloorPlans { get; set; } = new List<FloorPlan>();
}
