using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.UserInterface.Components;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LDropDownComponent : DropDownComponent, ILocalizedComponent
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

        protected LDropDownComponent(IModContext context, UIPanel panel, UILabel label, UIDropDown dropDown) : base(context, panel, label, dropDown)
        {
            Styles.Add(LocalizedComponent(this));
        }

        public static new LDropDownComponent Create(IModContext context, UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var dropDown = panel.Find<UIDropDown>("Dropdown");
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LDropDownComponent(context, panel, label, dropDown);
        }
    }
}