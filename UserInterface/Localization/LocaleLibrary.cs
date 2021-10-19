using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using Library = Dictionary<string, ILanguageDictionary>;

    public sealed class LocaleLibrary : WithContext, ILocaleLibrary, IManagedLifecycle
    {
        public static readonly ILocaleLibrary None = new DummyLocaleLibrary();

        private Library _availableLanguages;
        public Library AvailableLanguages
        {
            get => _availableLanguages;
            set => _availableLanguages = value ?? new Library();
        }

        private ILanguageDictionary _fallbackLanguage;
        public ILanguageDictionary FallbackLanguage
        {
            get => _fallbackLanguage;
            set => _fallbackLanguage = value ?? new LanguageDictionary(Context);
        }

        internal LocaleLibrary(IModContext context, Func<Library> initLibrary, Func<IModContext, ILanguageDictionary> initFallbackLanguage = null) : base(context)
        {
            _availableLanguages = new Library();
            _fallbackLanguage = new LanguageDictionary(context);
            _lifecycleManager = new LocaleLibraryLifecycleManager(context, this, initLibrary, initFallbackLanguage);
        }

        public ILanguageDictionary Get() => Get(null);
        public ILanguageDictionary Get(string key)
        {
            var targetKey = string.IsNullOrEmpty(key) ? LocaleConstants.DEFAULT_LANGUAGE_KEY : key;
            if (!(AvailableLanguages is null) && AvailableLanguages.TryGetValue(targetKey, out var dictionary))
            {
                return dictionary;
            }
            else
            {
                return FallbackLanguage;
            }
        }

        #region Lifecycle
        private readonly LocaleLibraryLifecycleManager _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private sealed class LocaleLibraryLifecycleManager : LifecycleManager
        {
            private readonly LocaleLibrary _library;
            private readonly Func<Library> _initLibrary;
            private readonly Func<IModContext, ILanguageDictionary> _initFallbackLanguage;

            public LocaleLibraryLifecycleManager(IModContext context, LocaleLibrary library, Func<Library> initLibrary, Func<IModContext, ILanguageDictionary> initFallbackLanguage = null) : base(context)
            {
                _library = library;
                _initLibrary = initLibrary;
                _initFallbackLanguage = initFallbackLanguage;
            }

            protected override bool OnInitialize()
            {
                if (_library is null) return false;
                if (!(_initLibrary is null)) _library.AvailableLanguages = _initLibrary();
                if (!(_initFallbackLanguage is null)) _library.FallbackLanguage = _initFallbackLanguage(Context);
                return true;
            }
            protected override bool OnTerminate() => true;
        }
        #endregion

        #region DummyLocaleLibrary
        private sealed class DummyLocaleLibrary : ILocaleLibrary
        {
            private readonly Library _availableLanguages = new Library();
            public Library AvailableLanguages
            {
                get
                {
                    _availableLanguages.Clear();
                    return _availableLanguages;
                }
            }

            public ILanguageDictionary FallbackLanguage
            {
                get => LanguageDictionary.None;
                set { }
            }

            public ILanguageDictionary Get() => LanguageDictionary.None;
            public ILanguageDictionary Get(string key) => LanguageDictionary.None;
        }
        #endregion
    }
}