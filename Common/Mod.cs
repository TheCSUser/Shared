using System;

namespace com.github.TheCSUser.Shared.Common
{
    internal sealed class Mod
    {
        public static readonly IMod None = new DummyMod();

        #region Dummy
        private sealed class DummyMod : IMod
        {
            public IModContext Context => ModContext.None;

            public string Name => "";
            public string Description => "";

            public event Action<object> Initialized { add { } remove { } }
            public event Action<object, string> SettingsChanged { add { } remove { } }
            public event Action<object> Terminating { add { } remove { } }
        }
        #endregion
    }
}
