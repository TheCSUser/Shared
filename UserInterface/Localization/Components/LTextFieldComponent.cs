using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    //todo
    internal class LTextFieldComponent : TextFieldComponent, ILocalizedComponent
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

        protected LTextFieldComponent(IModContext context, UIPanel panel, UILabel label, UITextField textField) : base(context, panel, label, textField)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LTextFieldComponent Create(IModContext context, UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var textField = panel.Find<UITextField>("Text Field");
            return new LTextFieldComponent(context, panel, label, textField);
        }
    }
}