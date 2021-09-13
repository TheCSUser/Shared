using com.github.TheCSUser.Shared.Logging;
using System;
using System.Threading;
using UnityEngine;

namespace com.github.TheCSUser.Shared.Common.Base
{
    public abstract class ScriptBase : IToggleable<bool>, IUpdatable<bool>, IErrorInfo, IDisposableEx, IManagedLifecycle
    {
        protected abstract string Name { get; }

        protected abstract bool OnEnable();
        protected abstract bool OnDisable();
        protected abstract bool OnUpdate();

        public virtual IInitializable GetLifecycleManager() => LifecycleManager.Empty;

        #region Error
#if DEV || PREVIEW
        private int ErrorTreshold = 1;
#else
        private int ErrorTreshold = 1;
#endif
        private int _errorCount;
        public int ErrorCount => _errorCount;
        public bool IsError
        {
            get
            {
                return _errorCount >= ErrorTreshold;
            }
            set
            {
                if (value) Interlocked.Add(ref _errorCount, ErrorTreshold);
                else _errorCount = 0;
            }
        }
        protected void IncreaseErrorCount() => Interlocked.Increment(ref _errorCount);
        protected void SetErrorTreshold(int value) => ErrorTreshold = value;
        #endregion

        #region Disposable
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
#if DEV
            Log.Info($"{GetType().Name}.{nameof(Dispose)}({nameof(disposing)}: {disposing}) called");
#endif
            if (disposing)
            {
                // dispose managed state (managed objects)

                if (IsEnabled) try
                    {
                        _isEnabled = !OnDisable();
                    }
#if DEV
                    catch (Exception e)
                    {

                        Log.Error($"{GetType().Name}.{nameof(Dispose)} call to {nameof(OnDisable)} failed", e);
                    }
#else
                    catch { /*ignore*/ }
#endif
                try
                {
                    var lm = GetLifecycleManager();
                    if (lm.IsInitialized) lm.Terminate();
                }
#if DEV
                catch (Exception e)
                {

                    Log.Error($"{GetType().Name}.{nameof(Dispose)} call to {nameof(LifecycleManagerBase.Terminate)} failed", e);
                }
#else
                catch { /*ignore*/ }
#endif
            }
            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _isDisposed = true;
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FeatureBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Toggleable
        private bool _isEnabled;

        public virtual bool IsEnabled => _isEnabled;

        public virtual bool Enable()
        {
            if (IsDisposed || IsError || !GetLifecycleManager().IsInitialized) return false;
            if (IsEnabled) return true;
            try
            {
#if DEV
                Log.Info($"{GetType().Name} enabling");
#endif
                _isEnabled = OnEnable();
                if (_isEnabled) CreateUpdater();
                return IsEnabled;
            }
            catch (Exception e)
            {
                _isEnabled = false;
                IncreaseErrorCount();
                Log.Error($"{GetType().Name}.{nameof(OnEnable)} failed", e);
                return false;
            }
        }
        public virtual bool Disable()
        {
            if (IsDisposed || IsError || !GetLifecycleManager().IsInitialized) return true;
            if (!IsEnabled) return true;
            try
            {
#if DEV
                Log.Info($"{GetType().Name} disabling");
#endif
                _isEnabled = !OnDisable();
                if (!_isEnabled) DestroyUpdater();
                return !IsEnabled;
            }
            catch (Exception e)
            {
                _isEnabled = true;
                IncreaseErrorCount();
                Log.Error($"{GetType().Name}.{nameof(OnDisable)} failed", e);
                return false;
            }
        }

        void IToggleable.Enable() => Enable();
        void IToggleable.Disable() => Disable();
        #endregion

        #region Updatable
        public virtual bool IsCurrent => false; //update always

        public virtual bool Update()
        {
            if (IsDisposed || IsError || !IsEnabled || !GetLifecycleManager().IsInitialized) return false;
            if (IsCurrent) return true;
            try
            {
#if DEV
                Log.Info($"{GetType().Name} updating");
#endif
                return OnUpdate();
            }
            catch (Exception e)
            {
                IncreaseErrorCount();
                Log.Error($"{GetType().Name}.{nameof(OnUpdate)} failed", e);
                return false;
            }
        }

        void IUpdatable.Update() => Update();
        #endregion

        #region Updater
        private GameObject UpdaterParent;

        protected virtual void CreateUpdater()
        {
            if (!(UpdaterParent is null)) DestroyUpdater();
#if DEV
            Log.Info($"{GetType().Name}.{nameof(CreateUpdater)} called");
#endif
            var name = string.IsNullOrEmpty(Name) ? $"{GetType().Name}.{nameof(Updater)}" : Name;
            var parent = new GameObject(name);
            var updater = parent.AddComponent<Updater>();
            updater._parent = this;
        }

        protected virtual void DestroyUpdater()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(DestroyUpdater)} called");
#endif
            var toDestroy = UpdaterParent;
            UpdaterParent = null;
            if (!(toDestroy is null)) UnityEngine.Object.Destroy(toDestroy);
        }

        internal class Updater : MonoBehaviour
        {
            internal ScriptBase _parent;

            public bool IsEnabled => !(_parent is null || _parent.UpdaterParent is null);

            public void Start()
            {
                if (_parent is null) return;
#if DEV
                Log.Info($"{_parent.GetType().Name}.{GetType().Name}.{nameof(Start)} called");
#endif
            }
            public void Update()
            {
                if (_parent is null) return;
                if (_parent.IsCurrent) return;
                try
                {
                    _parent.OnUpdate();
                }
                catch (Exception e)
                {
                    _parent.IncreaseErrorCount();
                    Log.Error($"{GetType().Name}.{nameof(Update)} failed", e);
                }
            }

            public void OnDestroy()
            {
                if (_parent is null) return;
#if DEV
                Log.Info($"{_parent.GetType().Name}.{GetType().Name}.{nameof(OnDestroy)} called");
#endif
                var toDestroy = _parent.UpdaterParent;
                _parent.UpdaterParent = null;
                if (!(toDestroy is null)) Destroy(toDestroy);
            }
        }
        #endregion
    }
}