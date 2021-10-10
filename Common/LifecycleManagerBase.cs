using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public static class LifecycleManager
    {
        public static IInitializable<bool> None = new DummyLifecycleManager();

        #region DummyLifecycleManager
        private sealed class DummyLifecycleManager : IInitializable<bool>
        {
            public bool IsInitialized { get; private set; }

            bool IInitializable<bool>.Initialize()
            {
                IsInitialized = true;
                return true;
            }

            void IInitializable.Initialize()
            {
                IsInitialized = true;
            }

            bool IInitializable<bool>.Terminate()
            {
                IsInitialized = false;
                return true;
            }

            void IInitializable.Terminate()
            {
                IsInitialized = false;
            }
        } 
        #endregion
    }

    public abstract class LifecycleManagerBase : IInitializable<bool>
    {
        protected abstract bool OnInitialize();
        protected abstract bool OnTerminate();

        public LifecycleManagerBase(IModContext context)
        {
            _context = context;
        }

        #region Context
        private readonly IModContext _context;

        protected IMod Mod => _context.Mod;
        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion

        #region Initializable
        private bool _isInitialized;
        public virtual bool IsInitialized => _isInitialized;

        public bool Initialize()
        {
            if (IsInitialized) return true;
            try
            {
                _isInitialized = OnInitialize();
                return IsInitialized;
            }
            catch (Exception e)
            {
                _isInitialized = false;
                Log.Error($"{GetType().Name}.{nameof(OnInitialize)} failed", e);
                return false;
            }
        }
        public bool Terminate()
        {
            if (!IsInitialized) return true;
            try
            {
                _isInitialized = !OnTerminate();
                return !IsInitialized;
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(OnTerminate)} failed", e);
                return false;
            }
        }

        void IInitializable.Initialize() => Initialize();
        void IInitializable.Terminate() => Terminate();
        #endregion
    }
}