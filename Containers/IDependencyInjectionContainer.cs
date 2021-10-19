using com.github.TheCSUser.Shared.Common;
using System;

namespace com.github.TheCSUser.Shared.Containers
{
    public interface IDependencyInjectionContainer
    {
        IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory) where T : class;
        IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory, bool singleton) where T : class;
        IDependencyInjectionContainer Register<T>(T singletonInstance) where T : class;
        IDependencyInjectionContainer Register(Type type, Delegate factory);
        IDependencyInjectionContainer Register(Type type, Delegate factory, bool singleton);
        IDependencyInjectionContainer Register(Type type, object instance);
        T Resolve<T>() where T : class;

        void Clear();
    }
}