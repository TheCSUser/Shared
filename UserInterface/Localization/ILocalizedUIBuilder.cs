using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using com.github.TheCSUser.Shared.UserInterface.Localization.Components;
using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface
{
    public interface ILocalizedUIBuilder
    {
        UIComponent Root { get; }
        ILocalizedGroup AddGroup(LocaleText text, float textScale = 1.0f, Color32? textColor = null);

        IButton AddButton(LocaleText text, Action<IButton> eventCallback, float textScale = 1.0f, Color32? textColor = null, bool enabled = true);
        ICheckbox AddCheckbox(LocaleText text, bool value, Action<ICheckbox, bool> eventCallback, float textScale = 1.0f, Color32? textColor = null, bool enabled = true);
        IDropDown AddDropdown(LocaleText text, string[] options, int defaultSelection, Action<IDropDown, int> eventCallback, float textScale = 1.0f, Color32? textColor = null, Color32? color = null, bool enabled = true);
        ILabel AddLabel(LocaleText text, float textScale = 1.0f, Color32? textColor = null);
        ISlider AddSlider(LocaleText text, float minValue, float maxValue, float stepSize, float value, Action<ISlider, float> eventCallback, float textScale = 1.0f, float width = 227.0f, Color32? textColor = null, Color32? color = null, bool enabled = true);
        ISpace AddSpace(float height);
    }
}
