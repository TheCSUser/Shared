using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public class LanguageDictionary : Dictionary<string, string>, IWithContext, ILanguageDictionary
    {
        public static readonly ILanguageDictionary None = new DummyLanguageDictionary();

        public LanguageDictionary(IModContext context) : base()
        {
            _context = context;
        }
        public LanguageDictionary(IModContext context, IDictionary<string, string> dictionary) : base(dictionary)
        {
            _context = context;
        }

        public virtual string Translate(string phrase, params string[] values)
        {
            { if (TryGetValue(phrase, out var translatedPhrase)) return SubstituteTokens(translatedPhrase, values); }
            { if (LocaleLibrary.Get().TryGetValue(phrase, out var translatedPhrase)) return SubstituteTokens(translatedPhrase, values); }

            return phrase;
        }

        protected string SubstituteTokens(string text, params string[] values)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (values is null || values.Length == 0) return text;
            string result = text;
            for (int i = 0; i < values.Length; i++)
            {
                result = result.Replace($"%{i + 1}", values[i]);
            }
            return result;
        }

        #region Context
        private readonly IModContext _context;

        protected IMod Mod => _context.Mod;
        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;

        public IModContext Context => _context;
        #endregion

        #region Dummy
        private class DummyLanguageDictionary : DummyCollection<KeyValuePair<string, string>>, ILanguageDictionary
        {
            public string this[string key] { get => ""; set { } }

            public ICollection<string> Keys => DummyCollection<string>.Instance;

            public ICollection<string> Values => DummyCollection<string>.Instance;

            public void Add(string key, string value) { }

            public bool ContainsKey(string key) => false;

            public bool Remove(string key) => false;

            public string Translate(string phrase, params string[] values) => phrase ?? "";

            public bool TryGetValue(string key, out string value)
            {
                value = null;
                return false;
            }
        }

        private class DummyCollection<T> : ICollection<T>
        {
            public static readonly DummyCollection<T> Instance = new DummyCollection<T>();

            public int Count => 0;
            public bool IsReadOnly => true;

            public void Add(T item) { }

            public void Clear() { }

            public bool Contains(T item) => false;

            public void CopyTo(T[] array, int arrayIndex) { }

            public bool Remove(T item) => false;

            public IEnumerator<T> GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        } 
        #endregion
    }
}
