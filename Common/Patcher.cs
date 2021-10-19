using com.github.TheCSUser.Shared.Properties;
using HarmonyLib;
using System.Collections.Generic;
using static CitiesHarmony.API.HarmonyHelper;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class Patcher : WithContext, IPatcher, IManagedLifecycle
    {
        public static IPatcher None = new DummyPatcher();
        internal static Patcher Shared = new Patcher(LibProperties.SharedContext, LibProperties.HarmonyId);

        private readonly Dictionary<string, int> Patches = new Dictionary<string, int>();

        public string HarmonyId { get; }

        internal Patcher(IModContext context, string harmonyId) : base(context)
        {
            HarmonyId = harmonyId;
            _lifecycleManager = new PatcherLifecycleManager(context, this);
        }

        private bool IsPatched(string patchId) => Patches.TryGetValue(patchId, out int value) && value > 0;

        public void Patch(PatchData patch)
        {
            if (patch is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Patch)} {nameof(patch)} is null.");
                return;
            }
            if (!GetLifecycleManager().IsInitialized)
            {
#if DEV
                Log.Info($"{nameof(Patcher)}.{nameof(Patch)} patch {patch.PatchId} will not be applied. {nameof(Patcher)} not initialized.");
#endif
                return;
            }
            var target = patch.Target?.Value;
            if (target is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Patch)} target for {patch.PatchId} is null.");
                return;
            }
            var prefix = patch.Prefix?.Value;
            var postfix = patch.Postfix?.Value;
            var transpiler = patch.Transpiler?.Value;
            var finalizer = patch.Finalizer?.Value;
            if (prefix is null
                && postfix is null
                && transpiler is null
                && finalizer is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Patch)} all patch methods for {patch.PatchId} are null");
                return;
            }
            if (IsPatched(patch.PatchId))
            {
#if DEV
                Log.Info($"{nameof(Patcher)}.{nameof(Patch)} patch {patch.PatchId} already applied.");
#endif
                return;
            }
#if DEV
            Log.Info($"{nameof(Patcher)}.{nameof(Patch)} applying harmony patch {patch.PatchId}.");
#endif
            if (Patches.ContainsKey(patch.PatchId))
            {
                Patches[patch.PatchId]++;
                if (Patches[patch.PatchId] > 1) return;
            }
            else
            {
                Patches.Add(patch.PatchId, 1);
            }

            DoOnHarmonyReady(() =>
            {
                try
                {
                    if (patch.IsApplied) return;
#if DEV
                    Log.Info($"{nameof(Patcher)}.{nameof(Patch)} Harmony ready, patching {patch.PatchId}.");
#endif
                    patch.IsApplied = true;
                    var harmony = new Harmony(HarmonyId);
                    harmony.Patch(target,
                        prefix is null ? null : new HarmonyMethod(prefix),
                        postfix is null ? null : new HarmonyMethod(postfix),
                        transpiler is null ? null : new HarmonyMethod(transpiler),
                        finalizer is null ? null : new HarmonyMethod(finalizer));
                    if (!(patch.OnPatch is null)) patch.OnPatch();
                }
                catch
                {
                    Log.Error($"{nameof(Patcher)}.{nameof(Patch)} {nameof(DoOnHarmonyReady)} for {patch.PatchId} failed");
                    patch.IsApplied = false;
                    throw;
                }
            });
        }
        public void Patch(IEnumerable<PatchData> patches)
        {
            foreach (var patch in patches) Patch(patch);
        }
        public void Unpatch(PatchData patch) => Unpatch(patch, false);
        public void Unpatch(PatchData patch, bool force)
        {
            if (patch is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Unpatch)} {nameof(patch)} is null.");
                return;
            }
            var target = patch.Target?.Value;
            if (target is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Unpatch)} target for {patch.PatchId} is null.");
                return;
            }
            var prefix = patch.Prefix?.Value;
            var postfix = patch.Postfix?.Value;
            var transpiler = patch.Transpiler?.Value;
            var finalizer = patch.Finalizer?.Value;
            if (prefix is null
                && postfix is null
                && transpiler is null
                && finalizer is null)
            {
                Log.Error($"{nameof(Patcher)}.{nameof(Unpatch)} all patch methods for {patch.PatchId} are null");
                return;
            }

            if (!IsPatched(patch.PatchId))
            {
#if DEV
                Log.Info($"{nameof(Patcher)}.{nameof(Unpatch)} patch {patch.PatchId} not applied.");
#endif
                patch.IsApplied = false;
                return;
            }
#if DEV
            Log.Info($"{nameof(Patcher)}.{nameof(Unpatch)} removing harmony patch {patch.PatchId}.");
#endif
            Patches[patch.PatchId]--;
            var useCount = Patches[patch.PatchId];

            if (useCount > 0 && !force) return;

            DoOnHarmonyReady(() =>
            {
                try
                {
                    if (!patch.IsApplied) return;
#if DEV
                    Log.Info($"{nameof(Patcher)}.{nameof(Unpatch)} Harmony ready, removing patch {patch.PatchId}.");
#endif
                    patch.IsApplied = false;
                    var harmony = new Harmony(HarmonyId);
                    if (!(prefix is null)) harmony.Unpatch(target, prefix);
                    if (!(postfix is null)) harmony.Unpatch(target, postfix);
                    if (!(transpiler is null)) harmony.Unpatch(target, transpiler);
                    if (!(finalizer is null)) harmony.Unpatch(target, finalizer);
                    if (!(patch.OnUnpatch is null)) patch.OnUnpatch();
                }
                catch
                {
                    Log.Error($"{nameof(Patcher)}.{nameof(Unpatch)} {nameof(DoOnHarmonyReady)} for {patch.PatchId} failed");
                    throw;
                }
            });
        }
        public void Unpatch(IEnumerable<PatchData> patches) => Unpatch(patches, false);
        public void Unpatch(IEnumerable<PatchData> patches, bool force = false)
        {
            foreach (var patch in patches) Unpatch(patch, force);
        }
        public void UnpatchAll()
        {
#if DEV
            Log.Info($"{nameof(Patcher)}.{nameof(UnpatchAll)} unpatching");
#endif
            DoOnHarmonyReady(() =>
            {
                try
                {
#if DEV
                    Log.Info($"{nameof(Patcher)}.{nameof(UnpatchAll)} Harmony ready, removing patches.");
#endif
                    var harmony = new Harmony(HarmonyId);
                    harmony.UnpatchAll(HarmonyId);
                    Patches.Clear();
                }
                catch
                {
                    Log.Error($"{nameof(Patcher)}.{nameof(UnpatchAll)} DoOnHarmonyReady failed");
                    throw;
                }
            });
        }

        #region Lifecycle
        private readonly IInitializable _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager;

        private sealed class PatcherLifecycleManager : LifecycleManager
        {
            private readonly Patcher _patcher;

            public PatcherLifecycleManager(IModContext context, Patcher patcher) : base(context)
            {
                _patcher = patcher;
            }

            protected override bool OnInitialize() => true;
            protected override bool OnTerminate()
            {
                _patcher.UnpatchAll();
                return true;
            }
        }
        #endregion

        #region DummyPatcher
        private sealed class DummyPatcher : IPatcher
        {
            public string HarmonyId => LibProperties.HarmonyId;

            public IInitializable GetLifecycleManager() => LifecycleManager.None;

            public void Patch(PatchData patch) { }
            public void Patch(IEnumerable<PatchData> patches) { }
            public void Unpatch(PatchData patch, bool force = false) { }
            public void Unpatch(IEnumerable<PatchData> patches, bool force = false) { }

            public void UnpatchAll() { }
        }
        #endregion
    }
}