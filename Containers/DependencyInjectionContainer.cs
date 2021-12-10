using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    public abstract class DependencyInjectionContainer : IDependencyInjectionContainer, IWithContext
    {
        private readonly Dictionary<Type, Delegate> _facories;
        private readonly Dictionary<Type, object> _instances;
        private readonly HashSet<Type> _singletons;

        protected abstract IModContext Context { get; }
        IModContext IWithContext.Context => Context;

        public DependencyInjectionContainer()
        {
            _facories = new Dictionary<Type, Delegate>();
            _instances = new Dictionary<Type, object>();
            _singletons = new HashSet<Type>();
        }

        public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory) where T : class => Register(factory, false);
        public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory, bool singleton) where T : class => Register(typeof(T), factory, singleton);
        public IDependencyInjectionContainer Register<T>(T instance) where T : class => Register(typeof(T), instance);
        public IDependencyInjectionContainer Register(Type type, Delegate factory) => Register(type, factory, false);
        public IDependencyInjectionContainer Register(Type type, Delegate factory, bool singleton)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (factory is null) throw new ArgumentNullException(nameof(factory));
            if (singleton && !_singletons.Contains(type)) _singletons.Add(type);
            _facories[type] = factory;
            return this;
        }
        public IDependencyInjectionContainer Register(Type type, object instance)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            if (!_singletons.Contains(type)) _singletons.Add(type);
            _instances[type] = instance;
            return this;
        }

        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (_instances.TryGetValue(type, out var instance)) return (T)instance;
            if (!_facories.TryGetValue(type, out var @delegate)) throw new ArgumentException($"Type {type.Name} not registered");
            var factory = (Func<IModContext, T>)@delegate;
            if (!_singletons.Contains(type)) return factory(Context);
            _instances[type] = instance = factory(Context);
            return (T)instance;
        }

        public void Clear()
        {
            _singletons.Clear();
            _instances.Clear();
            _facories.Clear();
        }

        #region Dummy
        protected abstract class DummyDependencyInjectionContainer : IDependencyInjectionContainer, IWithContext
        {
            protected abstract IModContext Context { get; }
            IModContext IWithContext.Context => Context;

            public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory) where T : class => this;
            public IDependencyInjectionContainer Register<T>(Func<IModContext, T> factory, bool singleton) where T : class => this;
            public IDependencyInjectionContainer Register<T>(T singletonInstance) where T : class => this;
            public IDependencyInjectionContainer Register(Type type, Delegate factory) => this;
            public IDependencyInjectionContainer Register(Type type, Delegate factory, bool singleton) => this;
            public IDependencyInjectionContainer Register(Type type, object instance) => this;
            public T Resolve<T>() where T : class => null;
            public void Clear() { }
        }
        #endregion
    }
}