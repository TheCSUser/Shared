﻿using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface IComponent : IDisposableContainer
    {
        T ApplyStyle<T>(Func<T, IDisposable> style) where T: IComponent, IWithContext;
        T ApplyStyle<T>(Action<T> style) where T: IComponent, IWithContext;
    }
}