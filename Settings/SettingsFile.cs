using com.github.TheCSUser.Shared.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.Settings
{
    public abstract class SettingsFile : INotifyPropertyChanged
    {
        [XmlIgnore]
        public Counter VersionCounter = new Counter();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (!(handler is null)) handler(this, new PropertyChangedEventArgs(propertyName));

        }
        protected void Set<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            VersionCounter.Update();
            RaisePropertyChanged(propertyName);
        }
        #endregion
    }
}
