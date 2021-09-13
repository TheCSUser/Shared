using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Imports;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using LanguageDictionary = Dictionary<string, string>;
    using Library = Dictionary<string, Dictionary<string, string>>;

    public static class LocaleLibrary
    {
        public static Library AvailableLanguages { get; set; }

        public static LanguageDictionary DefaultLanguage { get; set; }

        public static LanguageDictionary Get(string key)
        {
            var targetKey = string.IsNullOrEmpty(key) ? LocaleConstants.DEFAULT_LANGUAGE_KEY : key;
            if (!(AvailableLanguages is null) && AvailableLanguages.TryGetValue(targetKey, out var dictionary))
            {
                return dictionary;
            }
            else
            {
                return DefaultLanguage;
            }
        }

        #region Lifecycle
        private static readonly Lazy<IInitializable> _lifecycleManager = new Lazy<IInitializable>(() => new LifecycleManager());
        public static IInitializable GetLifecycleManager() => _lifecycleManager.Value;

        private class LifecycleManager : IInitializable
        {
            public bool IsInitialized { get; private set; }
            public void Initialize() => IsInitialized = true;
            public void Terminate() => IsInitialized = false;
        }
        #endregion
    }
}