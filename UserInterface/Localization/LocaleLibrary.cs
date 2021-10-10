using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    using Library = Dictionary<string, ILanguageDictionary>;

    public sealed class LocaleLibrary : ILocaleLibrary, IManagedLifecycle
    {
        public static readonly ILocaleLibrary None = new DummyLocaleLibrary();

        private Library _availableLanguages;
        public Library AvailableLanguages
        {
            get => _availableLanguages;
            set => _availableLanguages = value ?? new Library();
        }

        private ILanguageDictionary _defaultLanguage;
        public ILanguageDictionary DefaultLanguage
        {
            get => _defaultLanguage;
            set => _defaultLanguage = value ?? new LanguageDictionary(_context);
        }

        internal LocaleLibrary(IModContext context, Func<Library> initLibrary, Func<ILanguageDictionary> initDefaultLanguage = null)
        {
            _context = context;
            _availableLanguages = new Library();
            _defaultLanguage = new LanguageDictionary(context);
            _lifecycleManager = new LocaleLibraryLifecycleManager(context, this, initLibrary, initDefaultLanguage);
        }

        public ILanguageDictionary Get(string key = null)
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

        #region Context
        private readonly IModContext _context;
        #endregion

        #region Lifecycle
        private readonly LocaleLibraryLifecycleManager _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private sealed class LocaleLibraryLifecycleManager : LifecycleManagerBase
        {
            private readonly LocaleLibrary _library;
            private readonly Func<Library> _initLibrary;
            private readonly Func<ILanguageDictionary> _initDefaultLanguage;

            public LocaleLibraryLifecycleManager(IModContext context, LocaleLibrary library, Func<Library> initLibrary, Func<ILanguageDictionary> initDefaultLanguage = null) : base(context)
            {
                _library = library;
                _initLibrary = initLibrary;
                _initDefaultLanguage = initDefaultLanguage;
            }

            protected override bool OnInitialize()
            {
                if (_library is null) return false;
                if (!(_initLibrary is null)) _library.AvailableLanguages = _initLibrary();
                if (!(_initDefaultLanguage is null)) _library.DefaultLanguage = _initDefaultLanguage();
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

            public ILanguageDictionary Get(string key) => LanguageDictionary.None;
        }
        #endregion
    }
}