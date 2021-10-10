using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LCheckBoxComponent : CheckBoxComponent, ILocalizedComponent
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

        protected LCheckBoxComponent(IModContext context, UILabel label, UICheckBox checkBox) : base(context, label, checkBox)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LCheckBoxComponent Create(IModContext context, UIComponent root)
        {
            var checkBox = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UICheckBox;
            var label = checkBox.label;
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LCheckBoxComponent(context, label, checkBox);
        }
    }
}