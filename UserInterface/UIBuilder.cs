using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.UserInterface.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface
{
    public sealed class UIBuilder : WithContext, IUIBuilder, IDisposableContainer
    {
        private readonly UIComponent _root;
        public UIComponent Root
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
                return _root;
            }
        }

        internal UIBuilder(IModContext context, UIHelper helper, DisposableContainer disposables) : base(context)
        {
            _root = (UIComponent)helper.GetField("m_Root");
            _disposables = disposables;
        }
        internal UIBuilder(IModContext context, UIComponent root, DisposableContainer disposables) : base(context)
        {
            _root = root;
            _disposables = disposables;
        }

        public GroupComponent AddGroup(string text, float textScale = 1.0f, Color32? textColor = null)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = GroupComponent.Create(Context, Root, _disposables);
            component.Text = text;
            component.TextScale = textScale;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            _disposables.Add(component);
            return component;
        }

        public ButtonComponent AddButton(string text, Action<IButton> onClick, float textScale = 1.0f, Color32? textColor = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = ButtonComponent.Create(Context, Root);
            component.Text = text;
            component.TextScale = textScale;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            component.IsEnabled = enabled;
            if (!(onClick is null)) component.OnClick += onClick;
            _disposables.Add(component);
            return component;
        }
        public CheckBoxComponent AddCheckbox(string text, bool value, Action<ICheckbox, bool> onCheckedChanged, float textScale = 1.0f, Color32? textColor = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = CheckBoxComponent.Create(Context, Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            component.IsChecked = value;
            component.IsEnabled = enabled;
            if (!(onCheckedChanged is null)) component.OnCheckChanged += onCheckedChanged;
            _disposables.Add(component);
            return component;
        }
        public DropDownComponent AddDropdown(string text, string[] options, int defaultSelection, Action<IDropDown, int> onSelectedIndexChanged, float textScale = 1.0f, Color32? textColor = null, Color32? color = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = DropDownComponent.Create(Context, Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            component.Items = options;
            component.SelectedIndex = defaultSelection;
            if (!(onSelectedIndexChanged is null)) component.OnSelectedIndexChanged += onSelectedIndexChanged;
            if (color.HasValue) component.Color = color.Value;
            component.IsEnabled = enabled;
            _disposables.Add(component);
            return component;
        }
        public LabelComponent AddLabel(string text, float textScale = 1.0f, Color32? textColor = null)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = LabelComponent.Create(Context, Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            _disposables.Add(component);
            return component;
        }
        public SliderComponent AddSlider(string text, float minValue, float maxValue, float stepSize, float value, Action<ISlider, float> onValueChanged, float textScale = 1.0f, float width = 227.0f, Color32? textColor = null, Color32? color = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = SliderComponent.Create(Context, Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            if (color.HasValue) component.Color = color.Value;
            component.MinValue = minValue;
            component.MaxValue = maxValue;
            component.StepSize = stepSize;
            component.Value = value;
            component.Width = width;
            if (!(onValueChanged is null)) component.OnValueChanged += onValueChanged;
            component.IsEnabled = enabled;
            _disposables.Add(component);
            return component;
        }
        public SpaceComponent AddSpace(float height)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var component = SpaceComponent.Create(Context, Root);
            component.Height = height;
            return component;
        }

        #region DisposableContainer
        private DisposableContainer _disposables;
        void IDisposableContainer.Add(IDisposable disposable)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UIBuilder));
            var disposables = _disposables;
            if (disposable is null || disposables is null) return;
            disposables.Add(disposable);
        }
        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private IEnumerator<IDisposable> GetEnumerator()
        {
            var disposables = _disposables;
            if (disposables is null) return Enumerable.Empty<IDisposable>().GetEnumerator();
            return disposables.GetEnumerator();
        }
        #endregion

        #region Disposable
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _disposables = null;
        }
        #endregion

        #region UIBuilder
        IGroup IUIBuilder.AddGroup(string text, float textScale, Color32? textColor)
            => AddGroup(text, textScale, textColor);
        IButton IUIBuilder.AddButton(string text, Action<IButton> eventCallback, float textScale, Color32? textColor, bool enabled)
            => AddButton(text, eventCallback, textScale, textColor, enabled);
        ICheckbox IUIBuilder.AddCheckbox(string text, bool value, Action<ICheckbox, bool> eventCallback, float textScale, Color32? textColor, bool enabled)
            => AddCheckbox(text, value, eventCallback, textScale, textColor, enabled);
        IDropDown IUIBuilder.AddDropdown(string text, string[] options, int defaultSelection, Action<IDropDown, int> eventCallback, float textScale, Color32? textColor, Color32? color, bool enabled)
            => AddDropdown(text, options, defaultSelection, eventCallback, textScale, textColor, color, enabled);
        ILabel IUIBuilder.AddLabel(string text, float textScale, Color32? textColor)
            => AddLabel(text, textScale, textColor);
        ISlider IUIBuilder.AddSlider(string text, float minValue, float maxValue, float stepSize, float value, Action<ISlider, float> eventCallback, float textScale, float width, Color32? textColor, Color32? color, bool enabled)
            => AddSlider(text, minValue, maxValue, stepSize, value, eventCallback, textScale, width, textColor, color, enabled);
        ISpace IUIBuilder.AddSpace(float height)
            => AddSpace(height);
        #endregion
    }
}