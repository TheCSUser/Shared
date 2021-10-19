using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class ModContext : DependencyInjectionContainer, IModContext
    {
        public static readonly IModContext None = new DummyModContext();
        public static readonly IModContext Shared = new SharedModContext();

        private IMod _mod = Common.Mod.None;
        public IMod Mod
        {
            get => _mod;
            internal set
            {
                _mod = value ?? Common.Mod.None;
            }
        }

        private ILogger _log = Logging.Log.None;
        public ILogger Log
        {
            get => _log;
            internal set
            {
                _log = value ?? Logging.Log.None;
            }
        }

        private IPatcher _patcher = Common.Patcher.None;
        public IPatcher Patcher
        {
            get => _patcher;
            internal set
            {
                _patcher = value ?? Common.Patcher.None;
            }
        }

        private ILocaleLibrary _localeLibrary = UserInterface.Localization.LocaleLibrary.None;
        public ILocaleLibrary LocaleLibrary
        {
            get => _localeLibrary;
            internal set
            {
                _localeLibrary = value ?? UserInterface.Localization.LocaleLibrary.None;
            }
        }

        private ILocaleManager _localeManager = UserInterface.Localization.LocaleManager.None;
        public ILocaleManager LocaleManager
        {
            get => _localeManager;
            internal set
            {
                _localeManager = value ?? UserInterface.Localization.LocaleManager.None;
            }
        }

        protected override IModContext Context => this;

        internal ModContext() : base()
        {
            Register(_ => UserInterface.Localization.LocaleLibrary.None, true);
            Register(_ => UserInterface.Localization.LocaleManager.None, true);
            Register(_ => Logging.Log.None, true);
            Register(_ => Common.Mod.None, true);
            Register(_ => Common.Patcher.None, true);
        }

        #region Dummy
        private sealed class DummyModContext : IModContext
        {
            public ILocaleLibrary LocaleLibrary => UserInterface.Localization.LocaleLibrary.None;
            public ILocaleManager LocaleManager => UserInterface.Localization.LocaleManager.None;
            public ILogger Log => Logging.Log.None;
            public IMod Mod => Common.Mod.None;
            public IPatcher Patcher => Common.Patcher.None;

            public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory) where T : class => this;
            public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory, bool singleton) where T : class => this;
            public IDependencyInjectionContainer Register<T>(T singletonInstance) where T : class => this;
            public IDependencyInjectionContainer Register(Type type, Delegate factory) => this;
            public IDependencyInjectionContainer Register(Type type, Delegate factory, bool singleton) => this;
            public IDependencyInjectionContainer Register(Type type, object instance) => this;
            public T Resolve<T>() where T : class => null;
            public void Clear() { }
        }
        #endregion

        #region Shared
        private sealed class SharedModContext : DependencyInjectionContainer, IModContext
        {
            public ILocaleLibrary LocaleLibrary => UserInterface.Localization.LocaleLibrary.None;
            public ILocaleManager LocaleManager => UserInterface.Localization.LocaleManager.None;
            public ILogger Log => Logging.Log.Shared;
            public IMod Mod => Common.Mod.None;
            public IPatcher Patcher => Common.Patcher.None;

            protected override IModContext Context => this;

            public SharedModContext() : base()
            {
                Register(_ => UserInterface.Localization.LocaleLibrary.None, true);
                Register(_ => UserInterface.Localization.LocaleManager.None, true);
                Register(_ => Logging.Log.Shared, true);
                Register(_ => Common.Mod.None, true);
                Register(_ => Common.Patcher.None, true);
            }
        }
        #endregion
    }
}
