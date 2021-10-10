using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components.Base;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Components
{
    public class ButtonComponent : TextComponent, IButton
    {
        protected const string TemplateName = "OptionsButtonTemplate";

        public UIButton Button { get; }

        public event Action<IButton> OnClick;

        public bool IsEnabled { get => Button.isEnabled; set => Button.isEnabled = value; }

        protected ButtonComponent(IModContext context, UIButton button) : base(context, button)
        {
            button.eventClick += ButtonClickHandler;
            Button = button;
        }

        protected virtual void ButtonClickHandler(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (IsDisposed) return;
            try
            {
                var handler = OnClick;
                if (!(handler is null)) handler(this);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(ButtonComponent)}.{nameof(OnClick)} failed", e);
            }
        }

        public static ButtonComponent Create(IModContext context, UIComponent root)
        {
            var button = root.AttachUIComponent(UITemplateManager.GetAsGameObject(TemplateName)) as UIButton;
            button.autoSize = true;
            button.wordWrap = false;
            return new ButtonComponent(context, button);
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Button.eventClick -= ButtonClickHandler;
                OnClick = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}