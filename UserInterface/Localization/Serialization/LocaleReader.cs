using com.github.TheCSUser.Shared.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Serialization
{
    using Library = Dictionary<string, Dictionary<string, string>>;
    using static Path;

    public sealed class LocaleReader
    {
        private static readonly XmlSerializer Serializer;

        static LocaleReader()
        {
            Serializer = new XmlSerializer(typeof(LocaleFile));
        }

        public static Library Load(string path)
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
                        file = (Serializer.Deserialize(streamReader) as LocaleFile);
                    }
                    library.Add(
                        GetFileNameWithoutExtension(fileName).Split('.').Last().ToLower(),
                        file.ToDictionary(item => item.Name, item => item.Value));
                }
                return library;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(LocaleReader)}.{nameof(Load)} failed", e);
                return new Library();
            }
        }
    }
}