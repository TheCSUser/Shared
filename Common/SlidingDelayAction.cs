using com.github.TheCSUser.Shared.Properties;
using System;
using System.Threading;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class SlidingDelayAction<TParams> : IDisposableEx
    {
        private readonly object SynchRoot = new object();
        private Timer Timer = null;
        private int RemainingDelay;

        private readonly Action<TParams> Action;
        private readonly int Delay;
        private readonly int Resolution;

        private TParams Parameters;

        public SlidingDelayAction(Action<TParams> action) : this(action, 2, 1000) { }
        public SlidingDelayAction(Action<TParams> action, int delay, int resolution)
        {
            Action = action;
            Delay = delay;
            Resolution = resolution;
        }

        public void Invoke(TParams parameters)
        {
            if (_isDisposed) return;
            Parameters = parameters;
            Interlocked.Exchange(ref RemainingDelay, Delay);
            if (!(Timer is null)) return;
            lock (SynchRoot)
            {
                if (_isDisposed) return;
                if (!(Timer is null)) return;
                Timer = new Timer((state) =>
                {
                    try
                    {
                        if (Interlocked.Decrement(ref RemainingDelay) > 0) return;
                        Timer toDispose;
                        lock (SynchRoot)
                        {
                            toDispose = Timer;
                            Timer = null;
                        };
                        toDispose.Dispose();
                        if (!(Action is null)) Action(Parameters);
                    }
#if DEV
                    catch (Exception e)
#else
                    catch
#endif
                    {
#if DEV
                        LibProperties.SharedContext.Log.Error($"{GetType().Name}.{nameof(Timer)}.callback failed", e);
#else
                        //ignore
#endif
                    }
                }, null, 0, Resolution);
            }
        }

        #region Disposable
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) return;

            var timer = Timer;
            Timer = null;
            if (!(timer is null)) timer.Dispose();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
        #endregion

        public static implicit operator Action<TParams>(SlidingDelayAction<TParams> action) => (TParams @params) => action.Invoke(@params);
    }
}