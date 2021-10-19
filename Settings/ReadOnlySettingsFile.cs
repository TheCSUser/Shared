using System.ComponentModel;

namespace com.github.TheCSUser.Shared.Settings
{
    public abstract class ReadOnlySettingsFile : ISettings
    {
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { } remove { } }
    }
}
