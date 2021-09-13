using System.ComponentModel;

namespace com.github.TheCSUser.Shared.Settings
{
    public interface ISettingsReader
    {
        string FileName { get; }

        INotifyPropertyChanged Load();
    }

    public interface ISettingsWriter
    {
        string FileName { get; }

        void Delete();
        void Save(INotifyPropertyChanged data);
    }

    public interface ISettingsReader<TSettingsDto> : ISettingsReader
        where TSettingsDto : class, INotifyPropertyChanged
    {
        new TSettingsDto Load();
    }

    public interface ISettingsWriter<TSettingsDto> : ISettingsWriter
        where TSettingsDto : class, INotifyPropertyChanged
    {
        void Save(TSettingsDto data);
    }

    public interface ISettingsReaderWriter<TSettingsDto> : ISettingsReader<TSettingsDto>, ISettingsWriter<TSettingsDto>
        where TSettingsDto : class, INotifyPropertyChanged
    { }
}