using System.Globalization;

namespace Bajkuj.Localization;

public static partial class AppText
{
    public const string Slovak = "sk-SK";
    public const string English = "en-IE";

    public static string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public static string T(string text) => Language == "en" ? EnglishText.GetValueOrDefault(text, text) : text;

    public static string T(string text, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, T(text), arguments);
}
