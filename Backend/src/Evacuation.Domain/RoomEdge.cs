namespace Evacuation.Domain;

/// <summary>
/// Przejście między dwoma pomieszczeniami, wynikające z drzwi zaznaczonych na rzucie.
/// Krawędź jest nieskierowana — ewakuować można się w obie strony.
/// </summary>
public class RoomEdge
{
    public Guid Id { get; set; }

    public Guid FloorPlanId { get; set; }

    public FloorPlan? FloorPlan { get; set; }

    public Guid FromRoomId { get; set; }

    public Room? FromRoom { get; set; }

    public Guid ToRoomId { get; set; }

    public Room? ToRoom { get; set; }

    /// <summary>
    /// Długość przejścia w pikselach: od centroidu pierwszego pomieszczenia przez środek drzwi
    /// do centroidu drugiego. Na metry przelicza się dopiero przez skalę rzutu.
    /// </summary>
    public double WeightPixels { get; set; }
}
