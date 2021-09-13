using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public interface ISlider : ITextComponent
    {
        event Action<ISlider, float> OnValueChanged;
        event Action<ISlider, bool> OnIsEnabledChanged;

        float MinValue { get; set; }
        float MaxValue { get; set; }
        float StepSize { get; set; }
        float Value { get; set; }
        float Width { get; set; }
        bool IsEnabled { get; set; }
        Color32 Color { get; set; }
    }
}