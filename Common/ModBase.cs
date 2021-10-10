using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.EntryPoints;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.Settings;
using com.github.TheCSUser.Shared.UserInterface;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using com.github.TheCSUser.Shared.UserInterface.Localization.Serialization;
using ICities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace com.github.TheCSUser.Shared.Common
{
    using Library = Dictionary<string, ILanguageDictionary>;

    public abstract class ModBase : IMod
    {
        public event Action<object, string> SettingsChanged;
        public event Action<object> Initialized;
        public event Action<object> Terminating;

        private DisposableContainer _uiDisposables;
        private readonly InitializableContainer _initializables;

        public abstract string Name { get; }
        public abstract string Description { get; }

        public ModBase()
        {
            _context = new ModContext(this);
            _initializables = new InitializableContainer(_context);
            _localeReader = new LocaleReader(_context);
            _entryPoints = new Dictionary<ApplicationMode, IScriptContainer>();
        }

        public void OnEnabled()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnEnabled)}");
#endif
            try
            {
                foreach (var item in _initializables)
                    if (!(item is null)) item.Initialize();

                try
                {
                    var handler = Initialized;
                    if (!(handler is null)) handler(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"{GetType().Name}.{nameof(Initialized)} failed", ex);
                }
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnEnabled)} failed", e);
            }
        }
        public void OnDisabled()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnDisabled)}");
#endif            
            try
            {
                try
                {
                    var handler = Terminating;
                    if (!(handler is null)) handler(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"{GetType().Name}.{nameof(Terminating)} failed", ex);
                }

                foreach (var item in ((IEnumerable<IInitializable>)_initializables).Reverse())
                    if (!(item is null)) item.Terminate();

                SettingsChanged = null;
                Initialized = null;
                Terminating = null;
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnDisabled)} failed", e);
            }
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                if (!(_uiDisposables is null))
                {
                    _uiDisposables.Dispose();
                }

                if (helper is UIHelper concreteHelper)
                {
                    _uiDisposables = new DisposableContainer(_context);
                    BuildSettingsUI(
                        _useLocalization
                        ? new BuilderSelection(new LocalizedUIBuilder(_context, concreteHelper, _uiDisposables))
                        : new BuilderSelection(new UIBuilder(_context, concreteHelper, _uiDisposables))
                        );
                }
                else
                {
                    Log.Error($"{GetType().Name}.{nameof(OnSettingsUI)} invalid helper type {((helper is null) ? "null" : helper.GetType().Name)}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnSettingsUI)} failed", e);
            }
        }

        protected abstract void BuildSettingsUI(BuilderSelection builder);

        #region Context
        private readonly ModContext _context;
        public IModContext Context => _context;
        protected IPatcher Patcher => Context.Patcher;
        protected ILogger Log => Context.Log;
        protected ILocaleLibrary LocaleLibrary => Context.LocaleLibrary;
        protected ILocaleManager LocaleManager => Context.LocaleManager;
        #endregion

        #region Use
        protected void Use(IInitializable obj) => _initializables.Add(obj);
        protected T Use<T>(T obj) where T : IManagedLifecycle
        {
            _initializables.Add(obj.GetLifecycleManager());
            return obj;
        }
        protected void Use(Action onInitialize) => _initializables.Add(onInitialize);
        protected void Use(Action onInitialize, Action onTerminate) => _initializables.Add(onInitialize, onTerminate);
        #endregion

        #region UseLogger
        protected void UseLogger(string path) => UseLogger(path, GetType().Assembly.GetName().Name);
        protected void UseLogger(string path, string name)
        {
            _context.Log = new Log(path, name);
        }
        #endregion

        #region UseHarmony
        protected void UseHarmony()
        {
            var assemblyName = GetType().Assembly.GetName();
            UseHarmony($"{assemblyName.Name} {assemblyName.Version} {Guid.NewGuid()}");
        }
        protected void UseHarmony(string harmonyId)
        {
            Use(Common.Patcher.Shared);
            _context.Patcher = Use(new Patcher(_context, harmonyId));
        }
        #endregion

        #region UseLocalization
        private readonly LocaleReader _localeReader;
        private bool _useLocalization = false;
        protected void UseLocalization(string localeFilesPath, Func<ILanguageDictionary> initDefaultLanguage = null) => UseLocalization(() => _localeReader.Load(localeFilesPath), initDefaultLanguage);
        protected void UseLocalization(Func<Library> initLibrary, Func<ILanguageDictionary> initDefaultLanguage = null)
        {
            _context.LocaleLibrary = Use(new LocaleLibrary(_context, initLibrary, initDefaultLanguage));
            _context.LocaleManager = Use(new LocaleManager(_context));
            _useLocalization = true;
        }
        #endregion

        #region UseMode
        private readonly Dictionary<ApplicationMode, IScriptContainer> _entryPoints;
        protected IScriptContainer UseMode(ApplicationMode mode)
        {
            switch (mode)
            {
                case ApplicationMode.MainMenu:
                    {
                        if (!_entryPoints.TryGetValue(mode, out var entryPoint))
                        {
                            var mainMenuEntryPoint = new MainMenuEntryPoint(_context);
                            Use(mainMenuEntryPoint);
                            _entryPoints.Add(mode, mainMenuEntryPoint);
                            return mainMenuEntryPoint;
                        }
                        return entryPoint;
                    }
                case ApplicationMode.Game:
                case ApplicationMode.MapEditor:
                case ApplicationMode.AssetEditor:
                case ApplicationMode.ThemeEditor:
                case ApplicationMode.ScenarioEditor:
                    {
                        if (!_entryPoints.TryGetValue(mode, out var entryPoint))
                        {
                            var levelEntryPoint = new LevelEntryPoint(_context, mode);
                            Use(levelEntryPoint);
                            _entryPoints.Add(mode, levelEntryPoint);
                            return levelEntryPoint;
                        }
                        return entryPoint;
                    }
                default:
                    throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ApplicationMode));
            }
        }
        #endregion

        #region UseSettings
        private SettingsFile _settings;
        private ISettingsReader _settingsReader;
        private ISettingsWriter _settingsWriter;
        private Counter _settingsVersion;
#if DEV
        public Action LoadSettings;
#endif
        protected void UseSettings<TSettingsDto>(ISettingsReaderWriter<TSettingsDto> readerWriter, Action<TSettingsDto> onLoad = null)
            where TSettingsDto : SettingsFile, new() => UseSettings(readerWriter, readerWriter, onLoad);

        protected void UseSettings<TSettingsDto>(ISettingsReader<TSettingsDto> reader, ISettingsWriter<TSettingsDto> writer, Action<TSettingsDto> onLoad = null)
            where TSettingsDto : SettingsFile, new()
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));
            if (writer is null) throw new ArgumentNullException(nameof(writer));

            _settingsReader = reader;
            _settingsWriter = writer;

            void onTerminate()
            {
                if (!(_settings is null))
                {
                    _settingsVersion = _settings.VersionCounter.Clone();
                    _settings.PropertyChanged -= OnSettingsPropertyChanged;
                    _settings = null;
                }
            }
            void onInitialize()
            {
                var settings = reader.Load() ?? new TSettingsDto();

                onTerminate();

                if (!(settings is null))
                {
                    if (!(_settingsVersion is null))
                    {
                        _settingsVersion.Update();
                        settings.VersionCounter = _settingsVersion;
                    }
                    settings.PropertyChanged += OnSettingsPropertyChanged;
                    _settings = settings;
                }
                if (!(onLoad is null))
                {
                    try
                    {
                        onLoad(settings);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{GetType().Name}.{nameof(UseSettings)}.{nameof(onInitialize)}.{nameof(onLoad)} failed", e);
                    }
                }
                try
                {
                    var handler = SettingsChanged;
                    if (!(handler is null)) handler(settings, string.Empty);
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(UseSettings)}.{nameof(onInitialize)}.{nameof(SettingsChanged)} failed", e);
                }
            }
#if DEV
            LoadSettings = onInitialize;
#endif
            Use(onInitialize, onTerminate);
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                _settingsWriter.Save(_settings);
            }
            catch (Exception ex)
            {
                Log.Error($"{GetType().Name}.{nameof(OnSettingsPropertyChanged)} {nameof(ISettingsWriter)}.{nameof(ISettingsWriter.Save)} failed", ex);
            }
            try
            {
                var handler = SettingsChanged;
                if (!(handler is null)) handler(sender, e.PropertyName);
            }
            catch (Exception ex)
            {
                Log.Error($"{GetType().Name}.{nameof(SettingsChanged)} failed", ex);
            }
        }
        #endregion
    }
}