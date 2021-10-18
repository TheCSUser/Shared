using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;

namespace com.github.TheCSUser.Shared.Properties
{
    internal static class LibProperties
    {
        public const string HarmonyId = "com.github.TheCSUser.Shared";
        public const string Version = "1.0.2.0";
        public const int VersionInteger = 1;
        public const string Name = "TheCSUser Shared Library";
        public const string ShortName = "Shared";
#if DEV
        public const string Stream = "Dev";
        public const string LongName = Name + " " + Version + " " + Stream;
#elif PREVIEW
        public const string Stream = "Preview";
        public const string LongName = Name + " " + Version + " " + Stream;
#else
        public const string Stream = "";
        public const string LongName = Name + " " + Version;
#endif
        public const string Description = "Shared library for my mods";

        public static readonly IModContext SharedContext = new Context();

        private sealed class Context : ModContext.DummyModContext
        {
            public override ILogger Log => Logging.Log.Shared;
        }
    }
}
