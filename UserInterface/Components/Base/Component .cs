using System;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using System.Collections.Generic;
using System.Collections;

namespace com.github.TheCSUser.Shared.UserInterface.Components.Base
{
    public abstract class Component : WithContext, IDisposableEx, IComponent
    {
        protected readonly DisposableContainer Styles;

        protected Component(IModContext context) : base(context)
        {
            Styles = new DisposableContainer();
        }

        public T ApplyStyle<T>(Func<T, IDisposable> style)
            where T : IComponent, IWithContext
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (!(this is T derived)) throw new ArgumentException($"{GetType().Name} is not castable to {typeof(T).Name}", nameof(style), null);
            if (style is null) return derived;
            var cleanup = style(derived);
            if (!(cleanup is null)) Styles.Add(cleanup);
            return derived;
        }
        public T ApplyStyle<T>(Action<T> style)
            where T : IComponent, IWithContext
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (!(this is T derived)) throw new ArgumentException($"{GetType().Name} is not castable to {typeof(T).Name}", nameof(style), null);
            if (style is null) return derived;
            style(derived);
            return derived;
        }

        #region Disposable
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Styles.Dispose();
            }
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region DisposableContainer
        void IDisposableContainer.Add(IDisposable item)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ButtonComponent));
            Styles.Add(item);
        }
        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() => Styles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Styles.GetEnumerator();
        #endregion
    }
}