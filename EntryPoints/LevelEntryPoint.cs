using ColossalFramework;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Common.Base;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.Imports;
using com.github.TheCSUser.Shared.Logging;
using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace com.github.TheCSUser.Shared.EntryPoints
{
    internal class LevelEntryPoint : IScriptContainer, IDisposableEx, IManagedLifecycle
    {
        private readonly Cached<ILoading> _loadingManager = new Cached<ILoading>(() => Singleton<SimulationManager>.exists ? Singleton<SimulationManager>.instance.m_ManagersWrapper.loading : null);
        protected ApplicationMode CurrentMode => _loadingManager.Value is null ? ApplicationMode.MainMenu : _loadingManager.Value.currentMode.ToApplicationMode();

        protected readonly ApplicationMode RequiredMode;

        public LevelEntryPoint(ApplicationMode mode)
        {
            RequiredMode = mode;
            _lifecycleManager = new LifecycleManager(this);
        }

        protected virtual void OnEnable()
        {
            if (CurrentMode != RequiredMode) return;
#if DEV
            Log.Info($"{GetType().Name}.{nameof(OnEnable)} called");
#endif
            foreach (var script in Scripts)
            {
                if (script is null || script.IsEnabled) continue;
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
                if (script is null || !script.IsEnabled) continue;
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

        protected virtual void LevelLoadedHandler(SimulationManager.UpdateMode updateMode)
        {
            if (!_lifecycleManager.IsInitialized) return;
            OnEnable();
        }
        protected virtual void LevelPreUnloadedHandler()
        {
            if (!_lifecycleManager.IsInitialized) return;
            OnDisable();
        }

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
        private readonly IInitializable _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private class LifecycleManager : LifecycleManagerBase
        {
            private readonly LevelEntryPoint _parent;

            public LifecycleManager(LevelEntryPoint parent)
            {
                _parent = parent;
            }

            protected override bool OnInitialize()
            {
                Singleton<LoadingManager>.instance.m_levelLoaded += _parent.LevelLoadedHandler;
                Singleton<LoadingManager>.instance.m_levelPreUnloaded += _parent.LevelPreUnloadedHandler;
                ModBase.Initialized += ModInitializedHandler;
                ModBase.Terminating += ModTerminatingHandler;

                foreach (var script in _parent.Scripts)
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
                foreach (var script in ((IEnumerable<ScriptBase>)_parent.Scripts).Reverse())
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

                ModBase.Terminating -= ModTerminatingHandler;
                ModBase.Initialized -= ModInitializedHandler;
                Singleton<LoadingManager>.instance.m_levelLoaded -= _parent.LevelLoadedHandler;
                Singleton<LoadingManager>.instance.m_levelPreUnloaded -= _parent.LevelPreUnloadedHandler;
                return true;
            }

            protected void ModInitializedHandler(object obj) => _parent.OnEnable();
            protected void ModTerminatingHandler(object obj) => _parent.OnDisable();
        }
        #endregion
    }
}
