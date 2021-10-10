namespace com.github.TheCSUser.Shared.Common
{
    public interface IPatcher
    {
        string HarmonyId { get; }

        void Patch(PatchData patch);
        void Unpatch(PatchData patch, bool force = false);
        void UnpatchAll();
    }
}