﻿using ICities;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public interface IMod : IUserMod, IWithContext
    {
        event Action<object> Initialized;
        event Action<object, string> SettingsChanged;
        event Action<object> Terminating;
    }
}