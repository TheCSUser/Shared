namespace com.github.TheCSUser.Shared.Common
{
    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
        void Terminate();
    }
    public interface IInitializable<TResult>: IInitializable
    {
        new TResult Initialize();
        new TResult Terminate();
    }
}