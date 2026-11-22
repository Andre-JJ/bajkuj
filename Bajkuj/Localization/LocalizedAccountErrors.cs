using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bajkuj.Localization;

public static partial class AppText
{
    public static string AccountError(string message)
    {
        if (Language != "sk")
            return message;

        return message switch
        {
            "The First name field is required." => "Zadajte meno.",
            "The Last name field is required." => "Zadajte priezvisko.",
            "The field First name must be a string with a maximum length of 80." => "Meno môže mať najviac 80 znakov.",
            "The field Last name must be a string with a maximum length of 80." => "Priezvisko môže mať najviac 80 znakov.",
            "The Email field is required." => "Zadajte e-mail.",
            "The Password field is required." => "Zadajte heslo.",
            "The Confirm password field is required." => "Potvrďte heslo.",
            "The Email field is not a valid e-mail address." => "Zadajte platnú e-mailovú adresu.",
            "The password and confirmation password do not match." => "Heslá sa nezhodujú.",
            "Invalid login attempt." => "Prihlásenie sa nepodarilo.",
            "Verification email sent. Please check your email." => "Potvrdzovací e-mail bol odoslaný. Skontrolujte svoju schránku.",
            "Thank you for confirming your email." => "Ďakujeme za potvrdenie e-mailovej adresy.",
            "Error confirming your email." => "E-mailovú adresu sa nepodarilo potvrdiť.",
            "Invalid token." => "Odkaz už nie je platný.",
            "Passwords must have at least one non alphanumeric character." => "Heslo musí obsahovať aspoň jeden špeciálny znak.",
            "Passwords must have at least one digit ('0'-'9')." => "Heslo musí obsahovať aspoň jednu číslicu (0–9).",
            "Passwords must have at least one lowercase ('a'-'z')." => "Heslo musí obsahovať aspoň jedno malé písmeno (a–z).",
            "Passwords must have at least one uppercase ('A'-'Z')." => "Heslo musí obsahovať aspoň jedno veľké písmeno (A–Z).",
            _ => TranslateParameterizedAccountError(message)
        };
    }

    private static string TranslateParameterizedAccountError(string message)
    {
        if (message.StartsWith("User name '", StringComparison.Ordinal) && message.EndsWith("' is already taken.", StringComparison.Ordinal)
            || message.StartsWith("Email '", StringComparison.Ordinal) && message.EndsWith("' is already taken.", StringComparison.Ordinal))
            return "Účet s touto e-mailovou adresou už existuje.";

        var numbers = Regex.Matches(message, @"\d+");
        if (message.StartsWith("The Password must be at least ", StringComparison.Ordinal) && numbers.Count == 2)
            return $"Heslo musí mať od {numbers[0].Value} do {numbers[1].Value} znakov.";
        if (message.StartsWith("Passwords must be at least ", StringComparison.Ordinal) && numbers.Count == 1)
            return $"Heslo musí mať aspoň {numbers[0].Value} znakov.";
        if (message.StartsWith("Passwords must use at least ", StringComparison.Ordinal) && numbers.Count == 1)
            return $"Heslo musí obsahovať aspoň {numbers[0].Value} rôznych znakov.";
        return message;
    }
}

public sealed class LocalizedAccountErrors : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context) { }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        if (!string.Equals(context.RouteData.Values["area"] as string, "Identity", StringComparison.OrdinalIgnoreCase))
            return;

        foreach (var entry in context.ModelState.Values)
        {
            for (var i = 0; i < entry.Errors.Count; i++)
            {
                var error = entry.Errors[i];
                var translated = AppText.AccountError(error.ErrorMessage);
                if (translated != error.ErrorMessage)
                    entry.Errors[i] = new ModelError(translated);
            }
        }
    }
}

[HtmlTargetElement("input", Attributes = "asp-for")]
public sealed class LocalizedAccountInputTagHelper : TagHelper
{
    public override int Order => 0;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        foreach (var attribute in output.Attributes.ToArray())
        {
            if (!attribute.Name.StartsWith("data-val-", StringComparison.Ordinal) || attribute.Value is not string message)
                continue;

            var translated = AppText.AccountError(message);
            if (translated != message)
                output.Attributes.SetAttribute(attribute.Name, translated);
        }
    }
}
