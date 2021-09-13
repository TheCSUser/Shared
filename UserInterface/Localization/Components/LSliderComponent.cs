using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LSliderComponent : SliderComponent, ILocalizedComponent
    {
        private LocaleText _text;
        public new LocaleText Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                base.Text = value?.Translate() ?? string.Empty;
            }
        }

        protected LSliderComponent(UIPanel panel, UILabel label, UISlider slider) : base(panel, label, slider)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LSliderComponent Create(UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var slider = panel.Find<UISlider>("Slider");
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LSliderComponent(panel, label, slider);
        }
    }
}