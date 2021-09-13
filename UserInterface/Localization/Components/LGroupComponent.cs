using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    using static LocalizationStyles;

    public class LGroupComponent : TextComponent, ILocalizedGroup
    {
        protected const string TemplateName = "OptionsGroupTemplate";

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

        public UIPanel Panel { get; }
        public UILabel Label { get; }
        public LocalizedUIBuilder Builder { get; }

        protected LGroupComponent(UIPanel panel, UILabel label, LocalizedUIBuilder builder) : base(label)
        {
            Panel = panel;
            Label = label;
            Builder = builder;

            Styles.Add(LocalizedComponent(this));
        }

        public static LGroupComponent Create(UIComponent root, DisposableContainer disposables)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var content = panel.Find("Content");
            var builder = new LocalizedUIBuilder(content, disposables);
            return new LGroupComponent(panel, label, builder);
        }

        #region ILocalizedGroup
        ILocalizedUIBuilder ILocalizedGroup.Builder => Builder;
        #endregion
    }
}