using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
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

        protected GroupComponent(IModContext context, UIPanel panel, UILabel label, UIBuilder builder) : base(context, label)
        {
            Panel = panel;
            Label = label;
            Builder = builder;
        }

        public static GroupComponent Create(IModContext context, UIComponent root, DisposableContainer disposables)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var content = panel.Find("Content");
            var builder = new UIBuilder(context, content, disposables);
            return new GroupComponent(context, panel, label, builder);
        }

        #region IGroup
        IUIBuilder IGroup.Builder => Builder;
        #endregion
    }
}