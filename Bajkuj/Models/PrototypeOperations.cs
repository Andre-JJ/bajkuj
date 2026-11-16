namespace Bajkuj.Models;

public static class PrototypeOperations
{
    public const int PreviewTenantId = 1;
    public static string Money(decimal amount) => amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("sk-SK"));

    public record Payment(string Reference, string Contract, int TenantId, string Tenant, decimal Amount, int DueInDays, string Method, string Status);
    public static IReadOnlyList<Payment> Payments { get; } =
    [
        new("P-1042", "ZML-2025-042", 1, "Peter Kováč", 120m, -20, "Prevod", "Uhradená"),
        new("P-1051", "ZML-2025-042", 1, "Peter Kováč", 40m, 3, "Prevod", "Čaká na úhradu"),
        new("P-1036", "ZML-2025-036", 1, "Peter Kováč", 80m, -32, "Karta", "Uhradená"),
        new("P-1045", "ZML-2025-039", 4, "Lucia Benková", 120m, -4, "Karta", "Uhradená"),
        new("P-1048", "ZML-2025-040", 3, "Jakub Dvořák", 65m, -2, "Prevod", "Po splatnosti")
    ];

    public record Service(string Number, string Bike, string Issue, string Status, string Update, int? TenantId = null);
    public static IReadOnlyList<Service> Services { get; } =
    [
        new("SRV-103", "Cube Touring Hybrid ONE · #103", "Kontrola batérie", "V riešení", "Technik preveruje konektor a stav batérie."),
        new("SRV-023", "BK-023", "Nastavenie zadnej brzdy", "Prijatá", "Servisný tím potvrdí termín kontroly.", 1),
        new("SRV-019", "BK-019", "Výmena duše", "Vyriešená", "Duša bola vymenená a tlak skontrolovaný.", 1)
    ];

    public record Message(string Tenant, string Subject, string Body, string Reply, string Status);
    public static IReadOnlyList<Message> Messages { get; } =
    [
        new("Peter Kováč", "Vrátenie bicykla BK-023", "Môžem bicykel vrátiť v piatok po 16:00?", "Áno, bicykel prevezmeme na výdajnom mieste do 18:00.", "Zodpovedaná"),
        new("Peter Kováč", "Potvrdenie o úhrade", "Prosím o potvrdenie k platbe P-1042.", "Platbu evidujeme ako uhradenú v prehľade platieb.", "Zodpovedaná"),
        new("Jana Nováková", "Prevzatie mestského bicykla", "Kedy si môžem vyzdvihnúť rezervovaný bicykel?", "", "Čaká na odpoveď")
    ];

    public record Feedback(string Tenant, string Topic, int Rating, string Comment, string Status);
    public static IReadOnlyList<Feedback> FeedbackEntries { get; } =
    [
        new("Peter Kováč", "Prevzatie bicykla", 5, "Bicykel bol pripravený načas a prevzatie bolo rýchle.", "Prečítaná"),
        new("Tomáš Urban", "Vrátenie bicykla", 3, "Ocenil by som dlhšie otváracie hodiny cez víkend.", "Nová"),
        new("Lucia Benková", "Komunikácia", 4, "Pripomienka pred skončením prenájmu mi pomohla.", "Prečítaná")
    ];
}
