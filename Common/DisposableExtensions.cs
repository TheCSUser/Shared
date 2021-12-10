using System;

namespace com.github.TheCSUser.Shared.Common
{
    public static class DisposableExtensions
    {
        public static IDisposable AsDisposable(this Action action)
        {
            return new ActionOnDispose(action);
        }

        private sealed class ActionOnDispose : IDisposable
        {
            private Action _action;

            public ActionOnDispose(Action action)
            {
                _action = action;
            }
            public void Dispose()
            {
                if (!(_action is null))
                {
                    var action = _action;
                    _action = null;
                    try { action(); } catch { /*ignore*/ }
                }
            }
        }
    }
}