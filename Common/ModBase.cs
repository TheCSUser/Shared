using ColossalFramework;
using ColossalFramework.Plugins;
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
    using SettingsFile = Settings.SettingsFile;

    public abstract class ModBase : IMod
    {
        public event Action<object, string> SettingsChanged;

        private DisposableContainer _uiDisposables;
        private readonly InitializableContainer _initializables;
        private readonly List<Action> _onceInitializables;

        public abstract string Name { get; }
        public abstract string Description { get; }

        private bool _useLateInit = false;
        public bool IsEnabled { get; private set; }
        protected bool IsInitialLoad
        {
            get
            {
                try
                {
#if DEV
                    Log.Info($"{GetType().Name}.{nameof(IsInitialLoad)} Singleton<LoadingManager>.exists: {Singleton<LoadingManager>.exists}");
#endif
                    return !Singleton<LoadingManager>.exists;
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(IsInitialLoad)} failed", e);
                    return false;
                }
            }
        }

        public ModBase()
        {
            try
            {
                SharedDependencies.Initialize();
                PluginHelperProxy.OnPluginsValidated += OnPluginsValidated;

                _context = new ModContext
                {
                    Mod = this
                };

                _context
                .Register(GetType(), this)
                .Register(typeof(IMod), this);

                _initializables = new InitializableContainer(_context);
                _onceInitializables = new List<Action>();
                _localeReader = new LocaleReader(_context);
                _entryPoints = new Dictionary<ApplicationMode, IScriptContainer>();

                _context.Register(_localeReader);
            }
            catch (Exception e)
            {
                Logging.Log.Shared.Error($"{GetType().Name}.{nameof(ModBase)}.Constructor failed", e);
                throw;
            }
        }
        ~ModBase()
        {
            try
            {
                PluginHelperProxy.OnPluginsValidated -= OnPluginsValidated;
            }
            catch { /* ignore */ }
        }

        public void OnEnabled()
        {
            try
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnEnabled)} called");
#endif
                if (IsEnabled) return;
                IsEnabled = true;
                if (_useLateInit && IsInitialLoad && !PluginHelperProxy.PluginsValidated) return;
                OnEnable();
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnEnable)} failed", e);
            }
        }
        public void OnPluginsValidated()
        {
            try
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnPluginsValidated)} called");
#endif
                if (!_useLateInit) return;
                if (!IsEnabled) return;
                OnEnable();
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnEnable)} failed", e);
            }
        }
        public void OnDisabled()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            try
            {
                OnDisable();
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnDisable)} failed", e);
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
                    _uiDisposables = new DisposableContainer();
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

        protected virtual void OnEnable()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnEnable)} called");
#endif
            if (!(_onceInitializables is null) && _onceInitializables.Count > 0)
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnEnable)} running _onceInitializables");
#endif
                foreach (var item in _onceInitializables)
                    if (!(item is null)) item();
                _onceInitializables.Clear();
            }
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnEnable)} running _initializables Initialize actions");
#endif
            foreach (var item in _initializables)
                if (!(item is null)) item.Initialize();
        }
        protected virtual void OnDisable()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnDisable)} called");
            Log.Info($"{GetType().Name}.{nameof(OnDisable)} running _initializables Terminate actions");
#endif
            foreach (var item in ((IEnumerable<IInitializable>)_initializables).Reverse())
                if (!(item is null)) item.Terminate();

            SettingsChanged = null;
        }

        protected abstract void BuildSettingsUI(BuilderSelection builder);

        #region Context
        private readonly ModContext _context;
        public IModContext Context => _context;
        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion

        protected void UseLateInit() => _useLateInit = true;

        #region UseOnce
        protected void UseOnce(IInitializable obj) => _onceInitializables.Add(obj.Initialize);
        protected T UseOnce<T>(T obj) where T : IManagedLifecycle
        {
            _onceInitializables.Add(obj.GetLifecycleManager().Initialize);
            return obj;
        }
        protected void UseOnce(Action onInitialize) => _onceInitializables.Add(onInitialize);
        protected void UseOnce<TMod>(Action<TMod> onInitialize)
            where TMod : ModBase
        {
            if (onInitialize is null) return;
            UseOnce(() => onInitialize(this as TMod));
        }
        protected void UseOnce<TMod>(ICollection<Action<TMod>> actions)
            where TMod : ModBase
        {
            if (actions is null) return;
            foreach (var onInitialize in actions)
                UseOnce(onInitialize);
        }
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
        protected void Use<TMod>(Action<TMod> onInitialize)
            where TMod : ModBase
        {
            if (onInitialize is null) return;
            Use(() => onInitialize(this as TMod));
        }
        protected void Use<TMod>(ICollection<Action<TMod>> actions)
            where TMod : ModBase
        {
            if (actions is null) return;
            foreach (var onInitialize in actions)
                Use(onInitialize);
        }
        protected void Use<TMod>(Action<TMod> onInitialize, Action<TMod> onTerminate)
            where TMod : ModBase
        {
            if (onInitialize is null && onTerminate is null) return;
            Use(() => onInitialize(this as TMod), () => onTerminate(this as TMod));
        }
        #endregion

        #region UseLogger
        protected void UseLogger(string path) => UseLogger(path, GetType().Assembly.GetName().Name);
        protected void UseLogger(string path, string name)
        {
            var log = new Log(path, name);
            _context.Log = log;
            _context
            .Register(log)
            .Register<ILogger>(log);
        }
        #endregion

        #region UseHarmony
        protected void UseHarmony()
        {
            var assemblyName = GetType().Assembly.GetName();
            UseHarmony($"{assemblyName.Name} {assemblyName.Version} {Guid.NewGuid()}");
        }
        protected Patcher UseHarmony(string harmonyId)
        {
            var patcher = Use(new Patcher(_context, harmonyId));
            _context.Patcher = patcher;
            _context
            .Register(patcher)
            .Register<IPatcher>(patcher);
            return patcher;
        }
        #endregion

        #region UseLocalization
        private readonly LocaleReader _localeReader;
        private bool _useLocalization = false;
        protected void UseLocalization(string localeFilesPath, Func<IModContext, ILanguageDictionary> initFallbackLanguage = null) => UseLocalization(() => _localeReader.Load(localeFilesPath), initFallbackLanguage);
        protected void UseLocalization(Func<Library> initLibrary, Func<IModContext, ILanguageDictionary> initFallbackLanguage = null)
        {
            var localeLibrary = Use(new LocaleLibrary(_context, initLibrary, initFallbackLanguage));
            _context.LocaleLibrary = localeLibrary;
            var localeManager = Use(new LocaleManager(_context));
            _context.LocaleManager = localeManager;

            _context
            .Register(localeLibrary)
            .Register<ILocaleLibrary>(localeLibrary)
            .Register(localeManager)
            .Register<ILocaleManager>(localeManager);

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
        protected SettingsFile Settings => _settings;
        private ISettingsReader _settingsReader;
        private ISettingsWriter _settingsWriter;
        private Counter _settingsVersion;
#if DEV
        public Action LoadSettings;
#endif
        protected void UseSettings<TSettingsDto>(ISettingsReaderWriter<TSettingsDto> readerWriter)
            where TSettingsDto : SettingsFile, new()
        {
            _context.Register(readerWriter);
            UseSettings(readerWriter, readerWriter);
        }

        protected void UseSettings<TSettingsDto>(ISettingsReader<TSettingsDto> reader, ISettingsWriter<TSettingsDto> writer)
            where TSettingsDto : SettingsFile, new()
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));
            if (writer is null) throw new ArgumentNullException(nameof(writer));

            _settingsReader = reader;
            _settingsWriter = writer;

            _context
            .Register(reader)
            .Register(writer)
            .Register(reader.GetType(), reader)
            .Register(writer.GetType(), writer);

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
                    _context
                    .Register(settings)
                    .Register<SettingsFile>(settings);
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