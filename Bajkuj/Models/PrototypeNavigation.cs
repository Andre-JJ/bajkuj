namespace Bajkuj.Models;

public record PrototypeNavigation(string Section, string Label, string Icon, bool ShowInMenu = true)
{
    public static IReadOnlyList<PrototypeNavigation> Admin { get; } =
    [
        new("index", "Dashboard", "home"),
        new("zaujemcovia", "Záujemcovia", "people"),
        new("bicykle", "Bicykle", "bike"),
        new("najomcovia", "Nájomcovia", "person"),
        new("zmluvy", "Zmluvy", "file"),
        new("platby", "Platby", "card"),
        new("servis", "Servis", "wrench"),
        new("komunikacia", "Komunikácia", "mail"),
        new("prehlady", "Prehľady", "chart"),
        new("nastavenia", "Nastavenia", "settings"),
        new("aktivita", "Posledná aktivita", "history", ShowInMenu: false)
    ];

    public static IReadOnlyList<PrototypeNavigation> User { get; } =
    [
        new("index", "Domov", "home"),
        new("bicykle", "Bicykle", "bike"),
        new("zmluvy", "Zmluvy", "file"),
        new("platby", "Platby", "card"),
        new("servis", "Servis", "wrench"),
        new("komunikacia", "Komunikácia", "mail"),
        new("nastavenia", "Nastavenia", "settings")
    ];
}
