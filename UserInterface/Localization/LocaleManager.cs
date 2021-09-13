using ColossalFramework;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Common.Base;
using com.github.TheCSUser.Shared.Imports;
using com.github.TheCSUser.Shared.Logging;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using LanguageDictionary = Dictionary<string, string>;

    public static class LocaleManager
    {
        public static string GameLanguage => SingletonLite<ColossalFramework.Globalization.LocaleManager>.exists
            ? SingletonLite<ColossalFramework.Globalization.LocaleManager>.instance.language
            : "";
        public static event Action<string> LanguageChanged;
        public static bool UsingGameLanguage { get; private set; }

        private static LanguageDictionary _current;
        public static LanguageDictionary Current => _current ?? LocaleLibrary.DefaultLanguage;

        public static void ChangeTo(string key)
        {
#if DEV || PREVIEW
            Log.Info($"{nameof(LocaleManager)}.{nameof(ChangeTo)} setting language to {key}");
#endif
            var language = LocaleLibrary.Get(key);
            _current = language;
            UsingGameLanguage = false;
            var _event = LanguageChanged;
            if (!(_event is null)) _event(key);
        }
        public static void ChangeToGameLanguage()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(ChangeToGameLanguage)} setting language to game language");
#endif
            var key = GameLanguage;
            var language = LocaleLibrary.Get(key);
            _current = language;
            UsingGameLanguage = true;
            var _event = LanguageChanged;
            if (!(_event is null)) _event(key);
        }

        #region Events handling
        private static void OnEventUIComponentLocaleChanged()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(OnEventUIComponentLocaleChanged)} fired");
            Log.Info($"GameLanguage: {GameLanguage}");
#endif
            if (UsingGameLanguage) ChangeToGameLanguage();
        }

        private static void OnEventLocaleChanged()
        {
#if DEV
            Log.Info($"{nameof(LocaleManager)}.{nameof(OnEventLocaleChanged)} fired");
            Log.Info($"GameLanguage: {GameLanguage}");
#endif
            if (UsingGameLanguage) ChangeToGameLanguage();
        }
        #endregion

        #region Lifecycle
        private static readonly Lazy<IInitializable> _lifecycleManager = new Lazy<IInitializable>(() => new LifecycleManager());
        public static IInitializable GetLifecycleManager() => _lifecycleManager.Value;

        private class LifecycleManager : LifecycleManagerBase
        {
            protected override bool OnInitialize()
            {
                ColossalFramework.Globalization.LocaleManager.eventLocaleChanged += OnEventLocaleChanged;
                ColossalFramework.Globalization.LocaleManager.eventUIComponentLocaleChanged += OnEventUIComponentLocaleChanged;
                return true;
            }
            protected override bool OnTerminate()
            {
                ColossalFramework.Globalization.LocaleManager.eventLocaleChanged -= OnEventLocaleChanged;
                ColossalFramework.Globalization.LocaleManager.eventUIComponentLocaleChanged -= OnEventUIComponentLocaleChanged;
                return true;
            }
        }
        #endregion
    }
}