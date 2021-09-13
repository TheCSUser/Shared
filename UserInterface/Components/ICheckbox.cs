using ColossalFramework.UI;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface ICheckbox : ITextComponent
    {
        event Action<ICheckbox, bool> OnCheckChanged;

        bool IsChecked { get; set; }
        bool IsEnabled { get; set; }
    }
}