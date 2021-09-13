using ColossalFramework.UI;
using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface ITextComponent : IComponent
    {
        UITextComponent Component { get; }

        event Action<ITextComponent, string> OnTextChanged;

        string Text { get; set; }
        float TextScale { get; set; }
        Color32 TextColor { get; set; }
    }
}