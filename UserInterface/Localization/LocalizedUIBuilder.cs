using ColossalFramework.UI;
using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Containers;
using com.github.TheCSUser.Shared.UserInterface.Components;
using com.github.TheCSUser.Shared.UserInterface.Localization.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public sealed class LocalizedUIBuilder : ILocalizedUIBuilder, IDisposableContainer
    {
        private readonly UIComponent _root;
        public UIComponent Root
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
                return _root;
            }
        }

        internal LocalizedUIBuilder(UIHelper helper, DisposableContainer disposables)
        {
            _root = (UIComponent)helper.GetField("m_Root");
            _disposables = disposables;
        }
        internal LocalizedUIBuilder(UIComponent root, DisposableContainer disposables)
        {
            _root = root;
            _disposables = disposables;
        }

        public LGroupComponent AddGroup(LocaleText text, float textScale = 1.0f, Color32? textColor = null)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LGroupComponent.Create(Root, _disposables);
            component.Text = text;
            component.TextScale = textScale;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            _disposables.Add(component);
            return component;
        }

        public LButtonComponent AddButton(LocaleText text, Action<IButton> onClick, float textScale = 1.0f, Color32? textColor = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LButtonComponent.Create(Root);
            component.Text = text;
            component.TextScale = textScale;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            component.IsEnabled = enabled;
            if (!(onClick is null)) component.OnClick += onClick;
            _disposables.Add(component);
            return component;
        }
        public LCheckBoxComponent AddCheckbox(LocaleText text, bool value, Action<ICheckbox, bool> onCheckedChanged, float textScale = 1.0f, Color32? textColor = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LCheckBoxComponent.Create(Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            component.IsChecked = value;
            component.IsEnabled = enabled;
            if (!(onCheckedChanged is null)) component.OnCheckChanged += onCheckedChanged;
            _disposables.Add(component);
            return component;
        }
        public LDropDownComponent AddDropdown(LocaleText text, string[] options, int defaultSelection, Action<IDropDown, int> onSelectedIndexChanged, float textScale = 1.0f, Color32? textColor = null, Color32? color = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LDropDownComponent.Create(Root);
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
        public LLabelComponent AddLabel(LocaleText text, float textScale = 1.0f, Color32? textColor = null)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LLabelComponent.Create(Root);
            component.TextScale = textScale;
            component.Text = text;
            if (textColor.HasValue) component.TextColor = textColor.Value;
            _disposables.Add(component);
            return component;
        }
        public LSliderComponent AddSlider(LocaleText text, float minValue, float maxValue, float stepSize, float value, Action<ISlider, float> onValueChanged, float textScale = 1.0f, float width = 227.0f, Color32? textColor = null, Color32? color = null, bool enabled = true)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = LSliderComponent.Create(Root);
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
            if (IsDisposed) throw new ObjectDisposedException(nameof(LocalizedUIBuilder));
            var component = SpaceComponent.Create(Root);
            component.Height = height;
            return component;
        }

        #region IDisposableContainer
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

        #region IDisposable
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _disposables = null;
        }
        #endregion

        #region ILocalizedUIBuilder
        ILocalizedGroup ILocalizedUIBuilder.AddGroup(LocaleText text, float textScale, Color32? textColor)
    => AddGroup(text, textScale, textColor);
        IButton ILocalizedUIBuilder.AddButton(LocaleText text, Action<IButton> eventCallback, float textScale, Color32? textColor, bool enabled)
            => AddButton(text, eventCallback, textScale, textColor, enabled);
        ICheckbox ILocalizedUIBuilder.AddCheckbox(LocaleText text, bool value, Action<ICheckbox, bool> eventCallback, float textScale, Color32? textColor, bool enabled)
            => AddCheckbox(text, value, eventCallback, textScale, textColor, enabled);
        IDropDown ILocalizedUIBuilder.AddDropdown(LocaleText text, string[] options, int defaultSelection, Action<IDropDown, int> eventCallback, float textScale, Color32? textColor, Color32? color, bool enabled)
            => AddDropdown(text, options, defaultSelection, eventCallback, textScale, textColor, color, enabled);
        ILabel ILocalizedUIBuilder.AddLabel(LocaleText text, float textScale, Color32? textColor)
            => AddLabel(text, textScale, textColor);
        ISlider ILocalizedUIBuilder.AddSlider(LocaleText text, float minValue, float maxValue, float stepSize, float value, Action<ISlider, float> eventCallback, float textScale, float width, Color32? textColor, Color32? color, bool enabled)
            => AddSlider(text, minValue, maxValue, stepSize, value, eventCallback, textScale, width, textColor, color, enabled);
        object ILocalizedUIBuilder.AddSpace(float height)
            => AddSpace(height);
        #endregion
    }
}