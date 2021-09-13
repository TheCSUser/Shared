using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;
using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class SliderComponent : TextComponent, ISlider
    {
        protected const string TemplateName = "OptionsSliderTemplate";

        public UIPanel Panel { get; }
        public UILabel Label { get; }
        public UISlider Slider { get; }

        public event Action<ISlider, float> OnValueChanged;
        public event Action<ISlider, bool> OnIsEnabledChanged;

        public float MinValue { get => Slider.minValue; set => Slider.minValue = value; }
        public float MaxValue { get => Slider.maxValue; set => Slider.maxValue = value; }
        public float StepSize { get => Slider.stepSize; set => Slider.stepSize = value; }
        public float Value { get => Slider.value; set => Slider.value = value; }
        public float Width { get => Slider.width; set => Slider.width = value; }
        public bool IsEnabled { get => Slider.isEnabled; set => Slider.isEnabled = value; }
        public Color32 Color { get => Slider.color; set => Slider.color = value; }

        protected SliderComponent(UIPanel panel, UILabel label, UISlider slider) : base(label)
        {
            slider.eventValueChanged += ValueChangedHandler;
            slider.eventIsEnabledChanged += IsEnabledChangedHandler;
            Panel = panel;
            Label = label;
            Slider = slider;
        }

        protected virtual void ValueChangedHandler(UIComponent component, float eventParam)
        {
            if (IsDisposed) return;
            try
            {
                var handler = OnValueChanged;
                if (!(handler is null)) handler(this, eventParam);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(SliderComponent)}.{nameof(OnValueChanged)} failed", e);
            }
        }

        protected virtual void IsEnabledChangedHandler(UIComponent component, bool eventParam)
        {
            if (IsDisposed) return;
            try
            {
                var handler = OnIsEnabledChanged;
                if (!(handler is null)) handler(this, eventParam);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(SliderComponent)}.{nameof(OnIsEnabledChanged)} failed", e);
            }
        }

        public static SliderComponent Create(UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var slider = panel.Find<UISlider>("Slider");
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new SliderComponent(panel, label, slider);
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Slider.eventValueChanged -= ValueChangedHandler;
                Slider.eventIsEnabledChanged -= IsEnabledChangedHandler;
                OnValueChanged = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}