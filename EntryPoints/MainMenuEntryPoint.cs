using ColossalFramework;
using ColossalFramework.Packaging;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.Properties;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.github.TheCSUser.Shared.EntryPoints
{
    internal class MainMenuEntryPoint : IScriptContainer, IDisposableEx, IManagedLifecycle
    {
        private readonly Cached<ILoading> _loadingManager = new Cached<ILoading>(() => Singleton<SimulationManager>.exists ? Singleton<SimulationManager>.instance.m_ManagersWrapper.loading : null);
        protected ApplicationMode CurrentMode => _loadingManager.Value is null ? ApplicationMode.MainMenu : _loadingManager.Value.currentMode.ToApplicationMode();

        public bool IsEnabled { get; private set; }

        public MainMenuEntryPoint(IModContext context)
        {
            _context = context;
            _lifecycleManager = new MainMenuEntryPointLifecycleManager(context, this);
        }

        protected virtual void OnEnable()
        {
            if (CurrentMode != ApplicationMode.MainMenu) return;
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnEnable)} called");
#endif
            foreach (var script in Scripts)
            {
                if (script is null || script.IsDisposed || script.IsEnabled) continue;
                try
                {
                    script.Enable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnEnable)} call to {nameof(script)}.{nameof(ScriptBase.Enable)} failed", e);
                }
            }
        }
        protected virtual void OnDisable()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnDisable)} called");
#endif
            foreach (var script in ((IEnumerable<ScriptBase>)Scripts).Reverse())
            {
                if (script is null || script.IsDisposed || !script.IsEnabled) continue;
                try
                {
                    script.Disable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnDisable)} call to {nameof(script)}.{nameof(ScriptBase.Disable)} failed", e);
                }
            }
        }
        protected virtual void OnUpdate()
        {
            if (CurrentMode != ApplicationMode.MainMenu) return;
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnUpdate)} called");
#endif
            foreach (var script in Scripts)
            {
                if (script is null || script.IsDisposed || !script.IsEnabled || script.IsCurrent) continue;
                try
                {
                    script.Update();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnUpdate)} call to {nameof(script)}.{nameof(ScriptBase.Update)} failed", e);
                }
            }
        }

        public virtual void Enable()
        {
            if (!_lifecycleManager.IsInitialized) return;
            OnEnable();
        }
        public virtual void Disable()
        {
            if (!_lifecycleManager.IsInitialized) return;
            OnDisable();
        }
        public virtual void Update()
        {
            if (!_lifecycleManager.IsInitialized) return;
            OnUpdate();
        }

        #region Context
        private readonly IModContext _context;

        protected IMod Mod => _context.Mod;
        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion

        #region Disposable
        public bool IsDisposed { get; private set; }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                foreach (var script in ((IEnumerable<ScriptBase>)Scripts).Reverse())
                {
                    try
                    {
                        script.Dispose();
                    }
#if DEV
                    catch (Exception e)
                    {

                        Log.Error($"{GetType().Name}{nameof(Dispose)} call to {nameof(script)}.{nameof(ScriptBase.Dispose)} failed", e);
                    }
#else
                    catch { /*ignore*/ }
#endif
                }
                Scripts.Clear();
            }
            IsDisposed = true;
        }

        public void Dispose() => Dispose(true);
        #endregion

        #region ScriptContainer
        protected readonly List<ScriptBase> Scripts = new List<ScriptBase>();
        public IScriptContainer Add(ScriptBase item)
        {
            if (_lifecycleManager.IsInitialized) throw new InvalidOperationException($"Adding scripts is possible only when {nameof(LevelEntryPoint)} is not initialized");
            Scripts.Add(item);
            return this;
        }
        public IEnumerator<ScriptBase> GetEnumerator() => Scripts.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Scripts.GetEnumerator();
        #endregion

        #region Lifecycle
        private readonly MainMenuEntryPointLifecycleManager _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private class MainMenuEntryPointLifecycleManager : LifecycleManagerBase
        {
            protected MainMenuEntryPoint EntryPoint { get; }

            public MainMenuEntryPointLifecycleManager(IModContext context, MainMenuEntryPoint entryPoint) : base(context)
            {
                EntryPoint = entryPoint;
            }

            protected override bool OnInitialize()
            {
                Common.Patcher.Shared.Patch(MainMenuAwakePatch.Data);
                MainMenuAwakePatch.OnAwake += OnMainMenuAwake;
                Singleton<LoadingManager>.instance.m_levelPreLoaded += OnLevelPreLoaded;
                Mod.Initialized += ModInitializedHandler;
                Mod.Terminating += ModTerminatingHandler;

                foreach (var script in EntryPoint.Scripts)
                {
                    if (script is null || script.IsDisposed) continue;
                    var lifecycleManager = script.GetLifecycleManager();
                    if (lifecycleManager.IsInitialized) continue;
                    try
                    {
                        lifecycleManager.Initialize();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{GetType().Name}.{nameof(OnInitialize)} call to {nameof(lifecycleManager)}.{nameof(IInitializable.Initialize)} failed", e);
                    }
                }

                return true;
            }

            protected override bool OnTerminate()
            {
                foreach (var script in ((IEnumerable<ScriptBase>)EntryPoint.Scripts).Reverse())
                {
                    if (script is null || script.IsDisposed) continue;
                    var lifecycleManager = script.GetLifecycleManager();
                    if (!lifecycleManager.IsInitialized) continue;
                    try
                    {
                        lifecycleManager.Terminate();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{GetType().Name}.{nameof(OnInitialize)} call to {nameof(lifecycleManager)}.{nameof(IInitializable.Terminate)} failed", e);
                    }
                }

                Mod.Terminating -= ModTerminatingHandler;
                Mod.Initialized -= ModInitializedHandler;
                Singleton<LoadingManager>.instance.m_levelPreLoaded -= OnLevelPreLoaded;
                MainMenuAwakePatch.OnAwake -= OnMainMenuAwake;
                Common.Patcher.Shared.Unpatch(MainMenuAwakePatch.Data);
                return true;
            }

            protected void ModInitializedHandler(object obj) => EntryPoint.OnEnable();
            protected void ModTerminatingHandler(object obj) => EntryPoint.OnDisable();

            protected void OnMainMenuAwake()
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnMainMenuAwake)} enabling/updating.");
#endif
                try
                {
                    if (EntryPoint.IsEnabled) EntryPoint.Update();
                    else EntryPoint.Enable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnMainMenuAwake)} failed", e);
                }
            }
            protected void OnLevelPreLoaded()
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnLevelPreLoaded)} disabling.");
#endif
                try
                {
                    EntryPoint.Disable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnLevelPreLoaded)} failed", e);
                }
            }
        }
        #endregion
    }

    #region Harmony patch
    internal static class MainMenuAwakePatch
    {
        public static event Action OnAwake;

        public static readonly PatchData Data = new PatchData(
            patchId: $"{nameof(MainMenuEntryPoint)}.{nameof(MainMenuAwakePatch)}.{nameof(Postfix)}",
            target: () => typeof(MainMenu).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
            postfix: () => typeof(MainMenuAwakePatch).GetMethod(nameof(Postfix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Postfix()
        {
            if (!Data.IsPatchApplied) return;
#if DEV
            LibProperties.SharedContext.Log.Info($"{nameof(MainMenuAwakePatch)}.{nameof(Postfix)} enabling/updating {nameof(MainMenuEntryPoint)}.");
#endif
            try
            {
                var handler = OnAwake;
                if (handler is null) return;
                handler();
            }
            catch (Exception e)
            {
                LibProperties.SharedContext.Log.Error($"{nameof(MainMenuAwakePatch)}.{nameof(Postfix)} failed", e);
            }
        }
    }
    #endregion
}