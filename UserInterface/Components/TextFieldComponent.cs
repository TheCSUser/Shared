using ColossalFramework.UI;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    //todo
    internal class TextFieldComponent : TextComponent, ITextField
    {
        protected const string TemplateName = "OptionsTextfieldTemplate";

        public UIPanel Panel { get; }
        public UILabel Label { get; }
        public UITextField TextField { get; }

        protected TextFieldComponent(UIPanel panel, UILabel label, UITextField textField) : base(label)
        {
            Panel = panel;
            Label = label;
            TextField = textField;
        }

        public static TextFieldComponent Create(UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var textField = panel.Find<UITextField>("Text Field");
            return new TextFieldComponent(panel, label, textField);
        }
    }
}