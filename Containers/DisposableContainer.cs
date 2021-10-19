using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    public sealed class DisposableContainer : List<IDisposable>, IDisposableEx, IDisposableContainer
    {
        public DisposableContainer() : base() { }

        #region Disposable
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // dispose managed state (managed objects)
                foreach (var item in this)
                    if (!(item is null)) try
                        {
                            item.Dispose();
                        }
#if DEV
                        catch (Exception e)
                        {

                            Log.Shared.Error($"{nameof(DisposableContainer)}{nameof(Dispose)} failed", e);
                        }
#else
                        catch { /*ignore*/ }
#endif
                Clear();
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
    }
}