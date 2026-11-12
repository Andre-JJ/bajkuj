namespace Bajkuj.Models;

public record PrototypeContract(
    string Number, int TenantId, string Tenant, string Bicycle, DateOnly Start, DateOnly End, string Status)
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public static IReadOnlyList<PrototypeContract> CreateMockContracts()
    {
        var today = Today;

        return
        [
            new("ZML-2025-042", 1, "Peter Kováč", "BK-023", today.AddDays(-27), today.AddDays(3), "Aktívna"),
            new("ZML-2025-041", 2, "Mária Horváthová", "BK-031", today.AddDays(-25), today.AddDays(5), "Aktívna"),
            new("ZML-2025-040", 3, "Jakub Dvořák", "BK-018", today.AddDays(-22), today.AddDays(8), "Aktívna"),
            new("ZML-2025-039", 4, "Lucia Benková", "BK-027", today.AddDays(-20), today.AddDays(10), "Aktívna"),
            new("ZML-2025-038", 5, "Daniel Králik", "BK-035", today.AddDays(-17), today.AddDays(13), "Aktívna"),
            new("ZML-2025-043", 6, "Jana Nováková", "BK-046", today.AddDays(7), today.AddDays(37), "Čaká na podpis"),
            new("ZML-2025-037", 7, "Tomáš Urban", "BK-044", today.AddDays(-35), today.AddDays(-5), "Ukončená"),
            new("ZML-2025-036", 1, "Peter Kováč", "BK-019", today.AddDays(-60), today.AddDays(-30), "Ukončená")
        ];
    }
}
