using ColossalFramework;
using com.github.TheCSUser.Shared.Common;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public sealed class LocaleManager : WithContext, ILocaleManager, IManagedLifecycle
    {
        public static ILocaleManager None = new DummyLocaleManager();

        public event Action<string> LanguageChanged;

        public string GameLanguage => SingletonLite<ColossalFramework.Globalization.LocaleManager>.exists
            ? SingletonLite<ColossalFramework.Globalization.LocaleManager>.instance.language
            : "";
        public bool UsingGameLanguage { get; private set; }

        private string _currentKey;
        private ILanguageDictionary _current;
        public ILanguageDictionary Current => _current ?? LocaleLibrary.Get();

        internal LocaleManager(IModContext context) : base(context)
        {
            _lifecycleManager = new LocaleManagerLifecycleManager(context, this);
        }

        public void ChangeTo(string key)
        {
#if DEV || PREVIEW
            Log.Info($"{nameof(LocaleManager)}.{nameof(ChangeTo)} setting language to {key}");
#endif
            UsingGameLanguage = false;
            ChangeLanguage(key);
        }
        public void ChangeToGameLanguage()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(ChangeToGameLanguage)} setting language to game language");
#endif
            UsingGameLanguage = true;
            ChangeLanguage(GameLanguage);
        }
        private void ChangeLanguage(string key)
        {
            if (_currentKey == key) return;
            _currentKey = key;
            _current = LocaleLibrary.Get(key);
            var handler = LanguageChanged;
            if (!(handler is null)) handler(key);
        }

        #region Events handling
        private void OnEventUIComponentLocaleChanged()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(OnEventUIComponentLocaleChanged)} fired");
            Log.Info($"GameLanguage: {GameLanguage}");
#endif
            if (UsingGameLanguage) ChangeToGameLanguage();
        }

        private void OnEventLocaleChanged()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(OnEventLocaleChanged)} fired");
            Log.Info($"GameLanguage: {GameLanguage}");
#endif
            if (UsingGameLanguage) ChangeToGameLanguage();
        }
        #endregion

        #region Lifecycle
        private readonly LocaleManagerLifecycleManager _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private sealed class LocaleManagerLifecycleManager : LifecycleManager
        {
            private readonly LocaleManager _localeManager;

            public LocaleManagerLifecycleManager(IModContext context, LocaleManager localeManager) : base(context)
            {
                _localeManager = localeManager;
            }

            protected override bool OnInitialize()
            {
                ColossalFramework.Globalization.LocaleManager.eventLocaleChanged += _localeManager.OnEventLocaleChanged;
                ColossalFramework.Globalization.LocaleManager.eventUIComponentLocaleChanged += _localeManager.OnEventUIComponentLocaleChanged;
                return true;
            }
            protected override bool OnTerminate()
            {
                ColossalFramework.Globalization.LocaleManager.eventLocaleChanged -= _localeManager.OnEventLocaleChanged;
                ColossalFramework.Globalization.LocaleManager.eventUIComponentLocaleChanged -= _localeManager.OnEventUIComponentLocaleChanged;
                return true;
            }
        }
        #endregion

        #region Dummy
        private class DummyLocaleManager : ILocaleManager
        {
            public ILanguageDictionary Current => Localization.LanguageDictionary.None;

            public string GameLanguage => "";

            public bool UsingGameLanguage => false;

            public event Action<string> LanguageChanged { add { } remove { } }

            public void ChangeTo(string key) { }

            public void ChangeToGameLanguage() { }
        }
        #endregion
    }
}