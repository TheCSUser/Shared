using com.github.TheCSUser.Shared.Imports;
using System;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public class LocaleText
    {
        private readonly Func<string[]> _getValues;
        public string[] Values => _getValues is null ? null : _getValues();

        public string Phrase { get; }

        public LocaleText(string phrase)
        {
            Phrase = phrase;
        }
        public LocaleText(string phrase, params string[] values)
        {
            Phrase = phrase;
            if (!(values is null) && values.Length > 0)
            {
                _getValues = () => values;
            }
        }
        public LocaleText(string phrase, Func<string[]> getValues)
        {
            Phrase = phrase;
            _getValues = getValues;
        }

        public static implicit operator LocaleText(string phrase) => new LocaleText(phrase);
        public static implicit operator LocaleText(StringEnum phrase) => new LocaleText(phrase);
    }
}