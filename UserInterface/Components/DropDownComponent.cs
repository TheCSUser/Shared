using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;
using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class DropDownComponent : TextComponent, IDropDown
    {
        protected const string TemplateName = "OptionsDropdownTemplate";

        public UIPanel Panel { get; }
        public UILabel Label { get; }
        public UIDropDown DropDown { get; }

        public event Action<IDropDown, int> OnSelectedIndexChanged;

        public override Color32 TextColor
        {
            get => base.TextColor; set
            {
                DropDown.textColor = value;
                base.TextColor = value;
            }
        }
        public Color32 Color { get => DropDown.color; set => DropDown.color = value; }
        public string[] Items { get => DropDown.items; set => DropDown.items = value; }
        public int SelectedIndex { get => DropDown.selectedIndex; set => DropDown.selectedIndex = value; }
        public bool IsEnabled
        {
            get => DropDown.isInteractive;
            set
            {
                DropDown.isInteractive = value;
                DropDown.Invalidate();
            }
        }

        protected DropDownComponent(IModContext context, UIPanel panel, UILabel label, UIDropDown dropDown) : base(context, label)
        {
            dropDown.eventSelectedIndexChanged += SelectedIndexChangedHandler;
            Panel = panel;
            Label = label;
            DropDown = dropDown;
        }

        protected virtual void SelectedIndexChangedHandler(UIComponent component, int eventParam)
        {
            if (IsDisposed) return;
            try
            {
                var handler = OnSelectedIndexChanged;
                if (!(handler is null)) handler(this, eventParam);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(DropDownComponent)}.{nameof(OnSelectedIndexChanged)} failed", e);
            }
        }

        public static DropDownComponent Create(IModContext context, UIComponent root)
        {
            var panel = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIPanel;
            var label = panel.Find<UILabel>("Label");
            var dropDown = panel.Find<UIDropDown>("Dropdown");
            label.autoSize = true;
            label.autoHeight = false;
            label.wordWrap = false;
            return new DropDownComponent(context, panel, label, dropDown);
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                DropDown.eventSelectedIndexChanged -= SelectedIndexChangedHandler;
                OnSelectedIndexChanged = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}