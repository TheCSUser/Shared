using System;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public interface ILocaleManager
    {
        ILanguageDictionary Current { get; }
        string GameLanguage { get; }
        bool UsingGameLanguage { get; }

        event Action<string> LanguageChanged;

        void ChangeTo(string key);
        void ChangeToGameLanguage();
    }
}