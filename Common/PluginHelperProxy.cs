﻿using com.github.TheCSUser.Shared.Properties;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace com.github.TheCSUser.Shared.Common
{
    internal static class PluginHelperProxy
    {
        public static bool PluginsValidated { get; private set; } = false;

        public static event Action OnPluginsValidated;

        public static readonly PatchData ValidatePluginsPatch = new PatchData(
            patchId: $"Shared.{nameof(PluginHelperProxy)}.{nameof(ValidatePluginsPatch)}",
            target: () => typeof(PluginHelper).GetMethod("ValidatePlugins", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
            postfix: () => typeof(PluginHelperProxy).GetMethod(nameof(ValidatePluginsPostfix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
            onUnpatch: () => { OnPluginsValidated = null; }
        );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ValidatePluginsPostfix()
        {
            try
            {
                if (!ValidatePluginsPatch.IsApplied) return;
                try
                {
                    PluginsValidated = true;
                    var handler = OnPluginsValidated;
                    if (handler is null) return;
                    foreach (Action action in handler.GetInvocationList())
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            LibProperties.SharedContext.Log.Error($"{nameof(PluginHelperProxy)}.{nameof(ValidatePluginsPostfix)} failed", e);
                        }
                    }
                }
                catch (Exception e)
                {
                    LibProperties.SharedContext.Log.Error($"{nameof(PluginHelperProxy)}.{nameof(ValidatePluginsPostfix)} failed", e);
                }
            }
            catch { }
        }
    }
}