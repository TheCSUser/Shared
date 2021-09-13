using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class CheckBoxComponent : TextComponent, ICheckbox
    {
        protected const string TemplateName = "OptionsCheckBoxTemplate";

        public UILabel Label { get; }
        public UICheckBox CheckBox { get; }

        public event Action<ICheckbox, bool> OnCheckChanged;

        public bool IsChecked { get => CheckBox.isChecked; set => CheckBox.isChecked = value; }
        public bool IsEnabled { get => CheckBox.isEnabled; set => CheckBox.isEnabled = value; }

        protected CheckBoxComponent(UILabel label, UICheckBox checkBox) : base(label)
        {
            checkBox.eventCheckChanged += CheckChangedHandler;
            Label = label;
            CheckBox = checkBox;
        }

        protected virtual void CheckChangedHandler(UIComponent component, bool eventParam)
        {
            if (IsDisposed) return;
            try
            {
                var handler = OnCheckChanged;
                if (!(handler is null)) handler(this, eventParam);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(CheckBoxComponent)}.{nameof(OnCheckChanged)} failed", e);
            }
        }

        public static CheckBoxComponent Create(UIComponent root)
        {
            var checkBox = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UICheckBox;
            var label = checkBox.label;
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new CheckBoxComponent(label, checkBox);
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                CheckBox.eventCheckChanged -= CheckChangedHandler;
                OnCheckChanged = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}