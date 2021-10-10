using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class LabelComponent : TextComponent, ILabel
    {
        public UILabel Label { get; }

        protected LabelComponent(IModContext context, UILabel label) : base(context, label)
        {
            Label = label;
        }

        public static LabelComponent Create(IModContext context, UIComponent root)
        {
            var label = root.AddUIComponent<UILabel>();
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new LabelComponent(context, label);
        }
    }
}