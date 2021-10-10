using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    internal static class LocalizationStyles
    {
        public static IDisposable LocalizedComponent<T>(T component)
            where T : ILocalizedComponent, ITextComponent, IDisposableEx, IWithContext
        {
            ILogger Log() => component?.Context?.Log ?? Logging.Log.None;
            ILocaleManager LocaleManager() => component?.Context?.LocaleManager ?? Localization.LocaleManager.None;

            void languageChangedHandler(string key)
            {
                if (component.IsDisposed) return;
                try
                {
                    var localizedComponent = (ILocalizedComponent)component;
                    var textComponent = (ITextComponent)component;

                    if (localizedComponent.Text is null)
                    {
                        textComponent.Text = string.Empty;
                        return;
                    }

                    var translatedText = localizedComponent.Text is null ? "" : LocaleManager().Current.Translate(localizedComponent.Text.Phrase, localizedComponent.Text.Values);
#if DEV
                    Log().Info($"{component.GetType().Name}.{nameof(LocalizedComponent)} translating to {key}, phrase: {localizedComponent?.Text?.Phrase}, text: {translatedText}");
#endif
                    textComponent.Text = translatedText;
                }
                catch (Exception e)
                {
                    Log().Error($"{component.GetType().Name}.{nameof(LocalizedComponent)} failed", e);
                }
            }

            LocaleManager().LanguageChanged += languageChangedHandler;
            return DisposableExtensions.AsDisposable(() => LocaleManager().LanguageChanged -= languageChangedHandler);
        }
    }
}
