using com.github.TheCSUser.Shared.Common;
using System;
using System.IO;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.Settings
{
    public abstract class SettingsReaderWriter<TSettingsDto> : WithContext, ISettingsReaderWriter<TSettingsDto>
        where TSettingsDto : class, ISettings
    {
        public abstract string FileName { get; }

        private static readonly XmlSerializer Serializer;
        private static readonly XmlSerializerNamespaces Namespaces;

        static SettingsReaderWriter()
        {
            Serializer = new XmlSerializer(typeof(TSettingsDto));
            var noNamespaces = new XmlSerializerNamespaces();
            noNamespaces.Add("", "");
            Namespaces = noNamespaces;
        }

        public SettingsReaderWriter(IModContext context) : base(context) { }

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
                Log.Info($"{GetType().Name}.{nameof(Load)} {FileName} does not exist");
#endif
                return null;
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Load)} failed", e);
                return null;
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

        ISettings ISettingsReader.Load() => Load();

        void ISettingsWriter.Save(ISettings data) => Save(data as TSettingsDto);
    }
}