using com.github.TheCSUser.Shared.Common;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    public interface IInitializableContainer : IEnumerable<IInitializable>
    {
        void Add(IInitializable item);
    }
}