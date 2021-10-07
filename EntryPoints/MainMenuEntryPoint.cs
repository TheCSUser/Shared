﻿using ColossalFramework;
using ColossalFramework.Packaging;
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
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static com.github.TheCSUser.Shared.Common.Patcher;

namespace com.github.TheCSUser.Shared.EntryPoints
{
    internal class MainMenuEntryPoint : IScriptContainer, IDisposableEx, IManagedLifecycle
    {
        public static MainMenuEntryPoint Instance { get; private set; }

        private readonly Cached<ILoading> _loadingManager = new Cached<ILoading>(() => Singleton<SimulationManager>.exists ? Singleton<SimulationManager>.instance.m_ManagersWrapper.loading : null);
        protected ApplicationMode CurrentMode => _loadingManager.Value is null ? ApplicationMode.MainMenu : _loadingManager.Value.currentMode.ToApplicationMode();

        public bool IsEnabled { get; private set; }

        public MainMenuEntryPoint()
        {
            Instance = this;
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
            Instance = null;
            IsDisposed = true;
        }

        public void Dispose() => Dispose(true);

        ~MainMenuEntryPoint() => Dispose(false);
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
        private static readonly IInitializable _lifecycleManager = new LifecycleManager();
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private class LifecycleManager : LifecycleManagerBase
        {
            protected override bool OnInitialize()
            {
                Patch(MainMenuAwakePatch.Data);
                Patch(LoadingManagerLoadLevelPatch.Data);
                ModBase.Initialized += ModInitializedHandler;
                ModBase.Terminating += ModTerminatingHandler;

                foreach (var script in Instance.Scripts)
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
                foreach (var script in ((IEnumerable<ScriptBase>)Instance.Scripts).Reverse())
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
                Unpatch(LoadingManagerLoadLevelPatch.Data);
                Unpatch(MainMenuAwakePatch.Data);
                return true;
            }

            protected void ModInitializedHandler(object obj) => Instance.OnEnable();
            protected void ModTerminatingHandler(object obj) => Instance.OnDisable();
        }
        #endregion
    }

    #region Harmony patch
    internal static class MainMenuAwakePatch
    {
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
            Log.Info($"{nameof(MainMenuAwakePatch)}.{nameof(Postfix)} enabling/updating {nameof(MainMenuEntryPoint)}.");
#endif
            try
            {
                var instance = MainMenuEntryPoint.Instance;
                if (instance is null) return;
                if (instance.IsEnabled) instance.Update();
                else instance.Enable();
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(MainMenuAwakePatch)}.{nameof(Postfix)} failed", e);
            }
        }
    }

    internal static class LoadingManagerLoadLevelPatch
    {
        public static readonly PatchData Data = new PatchData(
            patchId: $"{nameof(MainMenuEntryPoint)}.{nameof(LoadingManagerLoadLevelPatch)}.{nameof(Prefix)}",
            target: () => typeof(LoadingManager).GetMethod("LoadLevel", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Package.Asset), typeof(string), typeof(string), typeof(SimulationMetaData), typeof(bool) }, null),
            prefix: () => typeof(LoadingManagerLoadLevelPatch).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Prefix(LoadingManager __instance)
        {
            if (!Data.IsPatchApplied) return true;
#if DEV
            Log.Info($"{nameof(LoadingManagerLoadLevelPatch)}.{nameof(Prefix)} disabling {nameof(MainMenuEntryPoint)}.");
#endif
            try
            {
                var instance = MainMenuEntryPoint.Instance;
                if (instance is null) return true;
                instance.Disable();
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(LoadingManagerLoadLevelPatch)}.{nameof(Prefix)} failed", e);
            }
            return true;
        }
    }
    #endregion
}