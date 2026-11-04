namespace Evacuation.Application.Graph;

/// <summary>
/// Połączenie między dwoma pomieszczeniami, wynikające z zaznaczonych na rzucie drzwi.
/// Krawędź jest nieskierowana — ewakuować można się w obie strony — więc dla jednoznaczności
/// zawsze zachodzi <c>FromRoomId &lt; ToRoomId</c>.
/// </summary>
/// <param name="WeightPixels">
/// Długość trasy przez te drzwi, w pikselach: od centroidu pierwszego pomieszczenia do środka
/// drzwi i dalej do centroidu drugiego. Liczenie „przez drzwi”, a nie w linii prostej między
/// centroidami, jest wyraźnie bliższe rzeczywistej drodze — prosta potrafi przechodzić przez ścianę.
///
/// Wartość zostaje w pikselach celowo. Przelicznik na metry jest własnością konkretnego rzutu
/// (<c>FloorPlan.PixelsPerMeter</c>) i nie należy do warstwy algorytmów.
/// </param>
public sealed record RoomEdge(int FromRoomId, int ToRoomId, double WeightPixels);
