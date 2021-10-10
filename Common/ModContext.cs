using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class ModContext : IModContext
    {
        public static readonly IModContext None = new DummyModContext();

        public IMod Mod { get; }

        private ILogger _log = Logging.Log.None;
        public ILogger Log
        {
            get => _log;
            set
            {
                _log = value ?? Logging.Log.None;
            }
        }

        private IPatcher _patcher = Common.Patcher.None;
        public IPatcher Patcher
        {
            get => _patcher;
            set
            {
                _patcher = value ?? Common.Patcher.None;
            }
        }

        private ILocaleLibrary _localeLibrary = UserInterface.Localization.LocaleLibrary.None;
        public ILocaleLibrary LocaleLibrary
        {
            get => _localeLibrary;
            set
            {
                _localeLibrary = value ?? UserInterface.Localization.LocaleLibrary.None;
            }
        }

        private ILocaleManager _localeManager = UserInterface.Localization.LocaleManager.None;
        public ILocaleManager LocaleManager
        {
            get => _localeManager;
            set
            {
                _localeManager = value ?? UserInterface.Localization.LocaleManager.None;
            }
        }

        internal ModContext(IMod mod)
        {
            Mod = mod;
        }

        #region Dummy
        internal class DummyModContext : IModContext
        {
            private readonly IMod _mod = Common.Mod.None;

            public virtual ILocaleLibrary LocaleLibrary => UserInterface.Localization.LocaleLibrary.None;
            public virtual ILocaleManager LocaleManager => UserInterface.Localization.LocaleManager.None;
            public virtual ILogger Log => Logging.Log.None;
            public virtual IMod Mod => _mod;
            public virtual IPatcher Patcher => Common.Patcher.None;
        }
        #endregion
    }
}
