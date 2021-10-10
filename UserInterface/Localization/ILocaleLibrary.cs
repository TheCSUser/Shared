using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using Library = Dictionary<string, ILanguageDictionary>;

    public interface ILocaleLibrary
    {
        Library AvailableLanguages { get; }

        ILanguageDictionary Get(string key = null);
    }
}