using com.github.TheCSUser.Shared.Properties;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.github.TheCSUser.Shared.Common
{
    internal static class MainMenuProxy
    {
        public static event Action OnAwake;

        public static readonly PatchData AwakePatch = new PatchData(
            patchId: $"Shared.{nameof(MainMenuProxy)}.{nameof(AwakePostfix)}",
            target: () => typeof(MainMenu).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
            postfix: () => typeof(MainMenuProxy).GetMethod(nameof(AwakePostfix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
            onUnpatch: () => { OnAwake = null; }
        );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AwakePostfix()
        {
            try
            {
                if (!AwakePatch.IsApplied) return;
                try
                {
                    var handler = OnAwake;
                    if (handler is null) return;
                    foreach (Action action in handler.GetInvocationList())
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            LibProperties.SharedContext.Log.Error($"{nameof(MainMenuProxy)}.{nameof(AwakePostfix)} failed", e);
                        }
                    }
                }
                catch (Exception e)
                {
                    LibProperties.SharedContext.Log.Error($"{nameof(MainMenuProxy)}.{nameof(AwakePostfix)} failed", e);
                }
            }
            catch { }
        }
    }
}