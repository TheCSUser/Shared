using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Imports;
using com.github.TheCSUser.Shared.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.Settings
{
    public class SettingsReaderWriter<TSettingsDto> : ISettingsReaderWriter<TSettingsDto>
        where TSettingsDto : class, INotifyPropertyChanged
    {
        private readonly Lazy<string> _fileName;
        public string FileName => _fileName.Value;

        private static readonly XmlSerializer Serializer;
        private static readonly XmlSerializerNamespaces Namespaces;

        static SettingsReaderWriter()
        {
            Serializer = new XmlSerializer(typeof(TSettingsDto));
            var noNamespaces = new XmlSerializerNamespaces();
            noNamespaces.Add("", "");
            Namespaces = noNamespaces;
        }

        public SettingsReaderWriter(IModContext context, Func<string> pathResolver)
        {
            _context = context;
            _fileName = new Lazy<string>(pathResolver);
        }

        public virtual void Save(TSettingsDto data)
        {
            if (data is null)
            {
#if DEV
                Log.Warning($"{GetType().Name}.{nameof(Save)} nothing to save");
#endif
                return;
            }

#if DEV
            Log.Info($"{GetType().Name}.{nameof(Save)} saving {FileName}");
#endif
            try
            {
                using (var streamWriter = new StreamWriter(FileName))
                {
                    Serializer.Serialize(streamWriter, data, Namespaces);
                }
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Save)} failed", e);
            }
        }

        public virtual TSettingsDto Load()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(Load)} loading settings");
#endif
            try
            {
                if (File.Exists(FileName))
                {
#if DEV
                    Log.Info($"{GetType().Name}.{nameof(Load)} loading {FileName}");
#endif
                    using (var streamReader = new StreamReader(FileName))
                    {
                        return (TSettingsDto)Serializer.Deserialize(streamReader);
                    }
                }
#if DEV
                Log.Info($"{GetType().Name}.{nameof(Load)} using new settings file");
#endif
                return default;
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Load)} failed", e);
                return default;
            }
        }

        public virtual void Delete()
        {
#if DEV
            Log.Info($"{GetType().Name}.{nameof(Delete)} deleting file {FileName}");
#endif
            try
            {
#if DEV || PREVIEW
                Log.Info($"{GetType().Name}.{nameof(Delete)} config files are preserved in dev and preview builds");
#else
                if (File.Exists(FileName)) File.Delete(FileName);
#endif
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Delete)} failed", e);
            }
        }

        INotifyPropertyChanged ISettingsReader.Load() => Load();

        void ISettingsWriter.Save(INotifyPropertyChanged data) => Save(data as TSettingsDto);

        #region Context
        private readonly IModContext _context;

        private ILogger Log => _context.Log;
        #endregion
    }
}