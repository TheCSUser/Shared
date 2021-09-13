using ColossalFramework.UI;
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
                base.Text = value?.Translate() ?? string.Empty;
            }
        }

        protected LCheckBoxComponent(UILabel label, UICheckBox checkBox) : base(label, checkBox)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LCheckBoxComponent Create(UIComponent root)
        {
            var checkBox = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UICheckBox;
            var label = checkBox.label;
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LCheckBoxComponent(label, checkBox);
        }
    }
}