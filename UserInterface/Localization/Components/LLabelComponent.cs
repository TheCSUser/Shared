using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LLabelComponent : LabelComponent, ILocalizedComponent
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

        protected LLabelComponent(UILabel label) : base(label)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LLabelComponent Create(UIComponent root)
        {
            var label = root.AddUIComponent<UILabel>();
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LLabelComponent(label);
        }
    }
}