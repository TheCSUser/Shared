using System;

namespace com.github.TheCSUser.Shared.Common
{
    public abstract class LifecycleManager : WithContext, IInitializable<bool>
    {
        public static IInitializable<bool> None = new DummyLifecycleManager();

        protected abstract bool OnInitialize();
        protected abstract bool OnTerminate();

        public LifecycleManager(IModContext context) : base(context) { }

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
}