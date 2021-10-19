using com.github.TheCSUser.Shared.Logging;

namespace com.github.TheCSUser.Shared.Common
{
    internal static class SharedDependencies
    {
        private static bool IsInitialized;

        public static void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
#if DEV
            Log.Shared.Info("Initializing shared dependencies.");
#endif
            Patcher.Shared.GetLifecycleManager().Initialize();

            Patcher.Shared.Patch(MainMenuProxy.AwakePatch);
            Patcher.Shared.Patch(PluginHelperProxy.ValidatePluginsPatch);
        }
    }
}