using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface IDropDown : ITextComponent
    {
        event Action<IDropDown, int> OnSelectedIndexChanged;

        Color32 Color { get; set; }
        string[] Items { get; set; }
        int SelectedIndex { get; set; }
        bool IsEnabled { get; set; }
    }
}