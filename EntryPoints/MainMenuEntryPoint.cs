﻿using ColossalFramework;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.github.TheCSUser.Shared.EntryPoints
{
    internal class MainMenuEntryPoint : WithContext, IScriptContainer, IDisposableEx, IManagedLifecycle
    {
        protected ApplicationMode CurrentMode
        {
            get
            {
                if (!Singleton<LoadingManager>.exists
                    || !Singleton<ToolManager>.exists
                    || Singleton<ToolManager>.instance.m_properties is null)
                    return ApplicationMode.MainMenu;
                try
                {
                    return Singleton<LoadingManager>
                        .instance
                        .m_LoadingWrapper
                        .currentMode
                        .ToApplicationMode();
                }
#if DEV
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(CurrentMode)}.Get failed", e);
#else
                catch
                {
#endif
                    return ApplicationMode.MainMenu;
                }
            }
        }

        public bool IsEnabled { get; private set; }

        public MainMenuEntryPoint(IModContext context) : base(context)
        {
            _lifecycleManager = new MainMenuEntryPointLifecycleManager(this);
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
                if (script is null || script.IsDisposed || !script.IsEnabled) continue;
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

        private sealed class MainMenuEntryPointLifecycleManager : LifecycleManager
        {
            private readonly MainMenuEntryPoint _parent;

            public MainMenuEntryPointLifecycleManager(MainMenuEntryPoint parent) : base(parent.Context)
            {
                _parent = parent;
            }

            protected override bool OnInitialize()
            {
                MainMenuProxy.OnAwake += OnMainMenuAwake;
                Singleton<LoadingManager>.instance.m_levelPreLoaded += OnLevelPreLoaded;

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
                _parent.OnEnable();

                return true;
            }

            protected override bool OnTerminate()
            {
                _parent.OnDisable();
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

                Singleton<LoadingManager>.instance.m_levelPreLoaded -= OnLevelPreLoaded;
                MainMenuProxy.OnAwake -= OnMainMenuAwake;
                return true;
            }

            private void OnMainMenuAwake()
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnMainMenuAwake)} enabling/updating.");
#endif
                try
                {
                    if (_parent.IsEnabled) _parent.Update();
                    else _parent.Enable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnMainMenuAwake)} failed", e);
                }
            }
            private void OnLevelPreLoaded()
            {
#if DEV
                Log.Info($"{GetType().Name}.{nameof(OnLevelPreLoaded)} disabling.");
#endif
                try
                {
                    _parent.Disable();
                }
                catch (Exception e)
                {
                    Log.Error($"{GetType().Name}.{nameof(OnLevelPreLoaded)} failed", e);
                }
            }
        }
        #endregion
    }
}