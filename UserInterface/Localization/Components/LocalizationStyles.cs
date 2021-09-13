using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Components;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    internal static class LocalizationStyles
    {
        public static IDisposable LocalizedComponent<T>(T component)
            where T : ILocalizedComponent, ITextComponent, IDisposableEx
        {
            void languageChangedHandler(string key)
            {
                if (component.IsDisposed) return;
                try
                {
                    if (((ILocalizedComponent)component).Text is null)
                    {
                        ((ITextComponent)component).Text = string.Empty;
                        return;
                    }
                    var translatedText = ((ILocalizedComponent)component).Text.Translate(key) ?? string.Empty;
#if DEV
                    Log.Info($"{component.GetType().Name}.{nameof(LocalizedComponent)} translating to {key}, phrase: {((ILocalizedComponent)component).Text.Phrase}, text: {translatedText}");
#endif
                    ((ITextComponent)component).Text = translatedText;
                }
                catch (Exception e)
                {
                    Log.Error($"{component.GetType().Name}.{nameof(LocalizedComponent)} failed", e);
                }
            }

            LocaleManager.LanguageChanged += languageChangedHandler;
            return DisposableExtensions.AsDisposable(() => LocaleManager.LanguageChanged -= languageChangedHandler);
        }
    }
}
