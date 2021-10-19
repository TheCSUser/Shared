using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LButtonComponent : ButtonComponent, ILocalizedComponent
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

        protected LButtonComponent(IModContext context, UIButton button) : base(context, button)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LButtonComponent Create(IModContext context, UIComponent root)
        {
            var button = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIButton;
            button.autoSize = true;
            button.wordWrap = false;
            return new LButtonComponent(context, button);
        }
    }
}