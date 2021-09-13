using ColossalFramework.UI;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface IButton : ITextComponent
    {
        event Action<IButton> OnClick;

        bool IsEnabled { get; set; }
    }
}