using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components;
using System;

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
                base.Text = value?.Translate() ?? string.Empty;
            }
        }

        protected LButtonComponent(UIButton button) : base(button)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LButtonComponent Create(UIComponent root)
        {
            var button = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIButton;
            button.autoSize = true;
            button.wordWrap = false;
            return new LButtonComponent(button);
        }
    }
}