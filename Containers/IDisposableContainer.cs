using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    public interface IDisposableContainer : IDisposable, IEnumerable<IDisposable>
    {
        void Add(IDisposable item);
    }
}