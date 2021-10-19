using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Components.Base
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Personal preference")]
    public abstract class TextComponent : Component, ITextComponent
    {
        private readonly UITextComponent _component;
        UITextComponent ITextComponent.Component => _component;

        public virtual string Text
        {
            get => _component.text;
            set
            {
                _component.text = value;
                TextChangedHandler(this, value);
            }
        }
        public virtual float TextScale { get => _component.textScale; set => _component.textScale = value; }
        public virtual Color32 TextColor { get => _component.textColor; set => _component.textColor = value; }

        private event Action<ITextComponent, string> _onTextChanged;
        public event Action<ITextComponent, string> OnTextChanged { add => _onTextChanged += value; remove => _onTextChanged -= value; }

        protected TextComponent(IModContext context, UITextComponent component) : base(context)
        {
            _component = component;
        }

        protected void TextChangedHandler(ITextComponent _, string text)
        {
            if (IsDisposed) return;
            try
            {
                var handler = _onTextChanged;
                if (!(handler is null)) handler(this, text);
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(ITextComponent.OnTextChanged)} failed", e);
            }
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _onTextChanged = null;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}