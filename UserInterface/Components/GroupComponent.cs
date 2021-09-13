using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class GroupComponent : TextComponent, IGroup
    {
        protected const string TemplateName = "OptionsGroupTemplate";

        public UIPanel Panel { get; }
        public UILabel Label { get; }
        public UIBuilder Builder { get; }

        protected GroupComponent(UIPanel panel, UILabel label, UIBuilder builder) : base(label)
        {
            Panel = panel;
            Label = label;
            Builder = builder;
        }

        public static GroupComponent Create(UIComponent root, DisposableContainer disposables)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var content = panel.Find("Content");
            var builder = new UIBuilder(content, disposables);
            return new GroupComponent(panel, label, builder);
        }

        #region IGroup
        IUIBuilder IGroup.Builder => Builder;
        #endregion
    }
}