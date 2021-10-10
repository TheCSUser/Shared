using com.github.TheCSUser.Shared.Common;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    public interface IScriptContainer: IEnumerable<ScriptBase>
    {
        IScriptContainer Add(ScriptBase item);
    }
}