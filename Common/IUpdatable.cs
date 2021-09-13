namespace com.github.TheCSUser.Shared.Common
{
    public interface IUpdatable
    {
        bool IsCurrent { get; }

        void Update();
    }

    public interface IForceUpdatable : IUpdatable
    {
        void Update(bool force);
    }

    public interface IUpdatable<TResult> : IUpdatable
    {
        new TResult Update();
    }
    public interface IForceUpdatable<TResult> : IUpdatable<TResult>, IForceUpdatable
    {
        new TResult Update(bool force);
    }
}