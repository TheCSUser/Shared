namespace com.github.TheCSUser.Shared.Checks
{
    public interface IPluginCheck
    {
        bool IsSubscribed { get; }
        bool IsNotSubscribed { get; }
        bool IsEnabled { get; }
        bool IsDisabled { get; }

        void Reset();
    }
}
