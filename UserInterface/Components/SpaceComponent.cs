using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class SpaceComponent: Component, ISpace
    {
        public UIPanel Panel { get; }

        public float Height { get => Panel.height; set => Panel.height = value; }

        protected SpaceComponent(UIPanel panel)
        {
            Panel = panel;
        }

        public static SpaceComponent Create(UIComponent root)
        {
            var panel = root.AddUIComponent<UIPanel>();
            panel.name = "Space";
            panel.isInteractive = false;
            return new SpaceComponent(panel);
        }
    }
}