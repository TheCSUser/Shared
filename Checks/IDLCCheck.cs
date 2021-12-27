namespace com.github.TheCSUser.Shared.Checks
{
    public interface IDLCCheck
    {
        bool IsAvailable { get; }
        bool IsNotAvailable { get; }
        bool IsEnabled { get; }
        bool IsDisabled { get; }
    }
}
