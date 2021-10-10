using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
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
                base.Text = value is null ? "" : LocaleManager.Current.Translate(value.Phrase, value.Values);
            }
        }

        protected LLabelComponent(IModContext context, UILabel label) : base(context, label)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LLabelComponent Create(IModContext context, UIComponent root)
        {
            var label = root.AddUIComponent<UILabel>();
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LLabelComponent(context, label);
        }
    }
}