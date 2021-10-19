namespace com.github.TheCSUser.Shared.Settings
{
    public interface ISettingsReader
    {
        string FileName { get; }

        ISettings Load();
    }

    public interface ISettingsWriter
    {
        string FileName { get; }

        void Delete();
        void Save(ISettings data);
    }

    public interface ISettingsReader<TSettingsDto> : ISettingsReader
        where TSettingsDto : class, ISettings
    {
        new TSettingsDto Load();
    }

    public interface ISettingsWriter<TSettingsDto> : ISettingsWriter
        where TSettingsDto : class, ISettings
    {
        void Save(TSettingsDto data);
    }

    public interface ISettingsReaderWriter<TSettingsDto> : ISettingsReader<TSettingsDto>, ISettingsWriter<TSettingsDto>
        where TSettingsDto : class, ISettings
    { }
}