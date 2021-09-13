namespace com.github.TheCSUser.Shared.Common
{
    public interface IToggleable
    {
        bool IsEnabled { get; }

        void Enable();
        void Disable();
    }

    public interface IForceToggleable : IToggleable
    {
        void Enable(bool force);
        void Disable(bool force);
    }

    public interface IToggleable<TResult>: IToggleable
    {
        new TResult Enable();
        new TResult Disable();
    }
    public interface IForceToggleable<TResult> : IToggleable<TResult>, IForceToggleable
    {
        new TResult Enable(bool force);
        new TResult Disable(bool force);
    }
}