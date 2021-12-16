using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Serialization
{
    using Library = Dictionary<string, ILanguageDictionary>;
    using static Path;

    public sealed class LocaleReader : WithContext
    {
        private static readonly string[] EMPTY_CHARACTERS = new[] {
            "\u0009",
            "\u000b",
            "\u000c",
            "\u0085",
            "\u00a0",
            "\u1680",
            "\u180e",
            "\u2000",
            "\u2001",
            "\u2002",
            "\u2003",
            "\u2004",
            "\u2005",
            "\u2006",
            "\u2007",
            "\u2008",
            "\u2009",
            "\u200a",
            "\u200b",
            "\u200c",
            "\u200d",
            "\u2028",
            "\u2029",
            "\u202f",
            "\u205f",
            "\u2060",
            "\u3000",
            "\ufeff"
        };

        private readonly XmlSerializer _serializer;

        internal LocaleReader(IModContext context) : base(context)
        {
            _serializer = new XmlSerializer(typeof(LocaleFile));
        }

        public Library Load(string path)
        {
#if DEV
            Log.Info($"{nameof(LocaleReader)}.{nameof(Load)} loading translations files from {path}");
#endif
            try
            {
                var library = new Library();
                foreach (var fileName in Directory
                    .GetFiles(path)
                    .Where(fn => GetExtension(fn) == ".xml"))
                {
#if DEV
                    Log.Info($"{nameof(LocaleReader)}.{nameof(Load)} loading {fileName}");
#endif
                    LocaleFile file;
                    using (var streamReader = new StreamReader(fileName))
                    {
                        file = (_serializer.Deserialize(streamReader) as LocaleFile);
                    }
                    library.Add(
                        GetFileNameWithoutExtension(fileName).Split('.').Last().ToLower(),
                        new LanguageDictionary(Context, file.ToDictionary(item => item.Name, item => PreProcess(item.Value))));
                }
                return library;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(LocaleReader)}.{nameof(Load)} failed", e);
                return new Library();
            }
        }

        private string PreProcess(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return EMPTY_CHARACTERS
                .Aggregate(text, (currentText, character) => currentText.Replace(character, " "))
                .Trim();
        }
    }
}