using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Common
{
    public interface IPatcher
    {
        string HarmonyId { get; }

        void Patch(PatchData patch);
        void Patch(IEnumerable<PatchData> patches);
        void Unpatch(PatchData patch, bool force = false);
        void Unpatch(IEnumerable<PatchData> patches, bool force = false);
        void UnpatchAll();
    }
}