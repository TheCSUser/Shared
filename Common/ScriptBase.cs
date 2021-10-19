using System;
using System.Threading;
using UnityEngine;

namespace com.github.TheCSUser.Shared.Common
{
    using ILogger = Logging.ILogger;

    public abstract class ScriptBase : WithContext, IToggleable<bool>, IUpdatable<bool>, IErrorInfo, IDisposableEx, IManagedLifecycle
    {
        protected abstract string Name { get; }

        protected abstract bool OnEnable();
        protected abstract bool OnDisable();
        protected abstract bool OnUpdate();

        public ScriptBase(IModContext context) : base(context) { }

        public virtual IInitializable GetLifecycleManager() => LifecycleManager.None;

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

                    Log.Error($"{GetType().Name}.{nameof(Dispose)} call to {nameof(LifecycleManager.Terminate)} failed", e);
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

        ~ScriptBase()
        {
            Dispose(disposing: false);
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
        public virtual bool Update()
        {
            if (IsDisposed || IsError || !IsEnabled || !GetLifecycleManager().IsInitialized) return false;
            try
            {
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
        private ScriptUpdater Updater;

        protected virtual ScriptUpdater AddUpdaterComponent(GameObject parent) => parent.AddComponent<ScriptUpdater>();

        private void CreateUpdater()
        {
            if (!(UpdaterParent is null)) DestroyUpdater();
#if DEV
            Log.Info($"{GetType().Name}.{nameof(CreateUpdater)} called");
#endif
            var name = string.IsNullOrEmpty(Name) ? $"{GetType().Name}.{nameof(ScriptUpdater)}" : Name;
            var parent = new GameObject(name);
            Updater = AddUpdaterComponent(parent);
            Updater._parent = this;
            Updater._context = Context;
        }

        private void DestroyUpdater()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(DestroyUpdater)} called");
#endif
            var updater = Updater;
            Updater = null;
            if (!(updater is null))
            {
                updater.enabled = false;
                updater._parent = null;
                updater._context = null;
                UnityEngine.Object.Destroy(updater);
            }

            var parent = UpdaterParent;
            UpdaterParent = null;
            if (!(parent is null)) UnityEngine.Object.Destroy(parent);
        }

        public class ScriptUpdater : MonoBehaviour
        {
            internal ScriptBase _parent;

            #region Context
            internal IModContext _context;
            protected IModContext Context => _context ?? ModContext.None;

            protected ILogger Log => _context?.Log ?? Logging.Log.None;
            #endregion

            #region MonoBehaviour
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
                OnUpdate();
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
            #endregion

            protected virtual void OnUpdate()
            {
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
        }
        #endregion
    }
}