using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using LanguageDictionary = Dictionary<string, string>;

    internal static class LocaleExtensions
    {
        public static string Translate(this LanguageDictionary instance, string phrase, params string[] values)
        {
            { if (!(instance is null) && instance.TryGetValue(phrase, out var translatedPhrase)) return SubstituteTokens(translatedPhrase, values); }
            { if (!(LocaleLibrary.DefaultLanguage is null) && LocaleLibrary.DefaultLanguage.TryGetValue(phrase, out var translatedPhrase)) return SubstituteTokens(translatedPhrase, values); }

            return phrase;
        }

        private static string SubstituteTokens(string instance, params string[] values)
        {
            if (string.IsNullOrEmpty(instance)) return string.Empty;
            if (values is null || values.Length == 0) return instance;
            string result = instance;
            for (int i = 0; i < values.Length; i++)
            {
                result = result.Replace($"%{i + 1}", values[i]);
            }
            return result;
        }
    }
}
