using System;

namespace com.github.TheCSUser.Shared.Common
{
    public interface IDisposableEx : IDisposable
    {
        bool IsDisposed { get; }
    }
}