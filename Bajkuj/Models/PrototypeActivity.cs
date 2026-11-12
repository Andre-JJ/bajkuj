namespace Bajkuj.Models;

public record PrototypeActivity(string Icon, string Description, string Detail, string Duration, string TimeLabel)
{
    public static IReadOnlyList<PrototypeActivity> Recent { get; } =
    [
        new("person", "Nový záujemca", "Jana Nováková", "PT10M", "pred 10 min"),
        new("file", "Uzavretá zmluva", "ZML-2025-042", "PT1H", "pred 1 hod"),
        new("card", "Prijatá platba", "120 € · ZML-2025-039", "PT2H", "pred 2 hod"),
        new("wrench", "Nahlásený servis", "Cube Touring Hybrid ONE · #103", "PT3H", "pred 3 hod"),
        new("mail", "Odoslaná správa", "Končiaci sa prenájom", "PT4H", "pred 4 hod"),
        new("bike", "Vydaný bicykel", "CTM Rambler 2.0 · #102", "P1D", "včera"),
        new("bike", "Vrátený bicykel", "Kellys Avenue 30 · #101", "P1D", "včera"),
        new("card", "Prijatá platba", "75 € · ZML-2025-038", "P2D", "pred 2 dňami")
    ];
}
