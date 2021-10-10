using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Serialization
{
    using Library = Dictionary<string, ILanguageDictionary>;
    using static Path;

    public sealed class LocaleReader
    {
        private readonly XmlSerializer _serializer;

        internal LocaleReader(IModContext context)
        {
            _serializer = new XmlSerializer(typeof(LocaleFile));
            _context = context;
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
                        new LanguageDictionary(_context, file.ToDictionary(item => item.Name, item => item.Value)));
                }
                return library;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(LocaleReader)}.{nameof(Load)} failed", e);
                return new Library();
            }
        }

        #region Context
        private readonly IModContext _context;

        private ILogger Log => _context.Log;
        #endregion
    }
}