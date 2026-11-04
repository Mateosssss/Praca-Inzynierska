namespace Evacuation.Api;

/// <summary>Odległości na powierzchni Ziemi.</summary>
internal static class Geo
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>Odległość po wielkim kole między dwoma punktami, w metrach.</summary>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2))
            + (Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));

        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    /// <summary>
    /// Zgrubne granice prostokąta obejmującego okrąg o zadanym promieniu.
    /// </summary>
    /// <remarks>
    /// Służy do wstępnego odsiania w zapytaniu SQL, które korzysta z indeksu na współrzędnych.
    /// Prostokąt jest szerszy niż okrąg, więc dokładną odległość liczy się potem już w pamięci,
    /// na garstce wierszy. Przy tysiącach budynków właściwym narzędziem byłby PostGIS.
    /// </remarks>
    public static (double MinLat, double MaxLat, double MinLon, double MaxLon) BoundingBox(
        double latitude,
        double longitude,
        double radiusMeters)
    {
        var latDelta = radiusMeters / 111_320.0;

        // Południki zbiegają się ku biegunom, więc stopień długości jest tym krótszy,
        // im dalej od równika. Ograniczenie cosinusa chroni przed dzieleniem przez zero.
        var cos = Math.Max(0.01, Math.Cos(latitude * Math.PI / 180));
        var lonDelta = radiusMeters / (111_320.0 * cos);

        return (latitude - latDelta, latitude + latDelta, longitude - lonDelta, longitude + lonDelta);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
